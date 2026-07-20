using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Prova.Api.Middlewares;
using Prova.Api.Tests.Infrastructure;
using Prova.Model.Enums;
using Prova.Service.Dtos;

namespace Prova.Api.Tests.Middlewares;

/// <summary>
/// Testes de integração do <see cref="ExceptionHandlingMiddleware"/> (T37).
///
/// Cobre os dois branches que os testes de controller existentes não
/// exercitavam (coverage de 68.7% antes desta task):
///   1. <c>catch (Exception ex)</c> genérico — nenhum teste existente disparava
///      uma exceção fora das quatro mapeadas (<see cref="FluentValidation.ValidationException"/>,
///      <see cref="Prova.Service.Exceptions.EntidadeNaoEncontradaException"/>,
///      <see cref="Prova.Service.Exceptions.EntidadeVinculadaException"/>,
///      <see cref="Prova.Service.Exceptions.CotacaoIndisponivelException"/>).
///   2. <c>if (context.Response.HasStarted)</c> — nenhum teste existente
///      provocava uma exceção depois que a resposta já tinha começado a ser
///      escrita.
///
/// De caminho, também cobre o <c>catch (ValidationException ex)</c> (400),
/// que nenhum teste de controller disparava (todos os testes existentes só
/// exercitavam o caminho feliz de criação/atualização) — sem isso o branch
/// coverage da classe não passa de ~87%.
///
/// Estratégia para o branch 1: registra <see cref="TesteErroController"/>
/// (existe só no projeto de teste) via ApplicationPart adicional no host de
/// teste. Esse controller lança uma <see cref="InvalidOperationException"/>
/// arbitrária — não mapeada por nenhum catch específico — forçando o
/// fallback 500. Roda tanto no ambiente padrão de teste quanto em
/// "Development" para provar que o comentário do middleware ("nunca expõe
/// stack trace/mensagem crua ao cliente, em nenhum ambiente") vale na
/// prática, não só na intenção documentada.
///
/// Estratégia para o branch 2: registra um <see cref="IStartupFilter"/> que
/// insere um middleware ANTES do <see cref="ExceptionHandlingMiddleware"/> no
/// pipeline (antes de <c>next(app)</c>, que é o pipeline real definido em
/// <c>Program.cs</c>). Esse middleware escreve um byte na resposta (fazendo
/// <c>HasStarted</c> virar <c>true</c>) e então lança, simulando uma exceção
/// que ocorre depois que o cliente já começou a receber a resposta (ex.:
/// streaming) — único jeito de exercitar esse branch sem alterar o código de
/// produção do middleware.
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ExcecaoNaoMapeada_Retorna500SemVazarMensagemOuStackTrace()
    {
        using var factory = CriarFactoryComControllerDeErro();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/teste-erro");

        await AssertarRespostaGenericaSemVazamentoAsync(resposta);
    }

    [Fact]
    public async Task InvokeAsync_ExcecaoNaoMapeadaEmDevelopment_TambemRetorna500SemVazarMensagemOuStackTrace()
    {
        using var factory = CriarFactoryComControllerDeErro(ambiente: "Development");
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/teste-erro");

        await AssertarRespostaGenericaSemVazamentoAsync(resposta);
    }

    [Fact]
    public async Task InvokeAsync_ValidationExceptionDoFluentValidation_Retorna400ComMensagensDeValidacao()
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var dtoInvalido = new CarneDto(string.Empty, OrigemCarne.Bovina);

        var resposta = await client.PostAsJsonAsync("/api/carnes", dtoInvalido);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpoTexto = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("   at ", corpoTexto); // assinatura de stack trace serializado

        var corpo = await resposta.Content.ReadFromJsonAsync<ErroRespostaDto>();
        Assert.NotNull(corpo);
        Assert.NotEmpty(corpo!.Erros);
        Assert.Contains(corpo.Erros, m => m.Contains("obrigatória"));
    }

    [Fact]
    public async Task InvokeAsync_ExcecaoAposRespostaJaIniciada_NaoTentaReescreverCorpoJaEnviado()
    {
        using var factoryBase = new ProvaApiFactory();
        using var factory = factoryBase.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
                services.AddSingleton<IStartupFilter, RespostaJaIniciadaStartupFilter>());
        });
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync(RespostaJaIniciadaStartupFilter.Caminho);

        // A resposta já tinha começado a ser escrita (200, corpo parcial)
        // quando a exceção ocorreu — o middleware detecta HasStarted==true e
        // não tenta sobrescrever status/corpo (branch curto-circuitado em
        // EscreverRespostaAsync). O erro completo (com stack trace) ainda é
        // logado no servidor — o teste não tem acesso ao logger aqui, mas
        // isso já é garantido pela chamada incondicional a _logger.LogError
        // antes da checagem de HasStarted.
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpoTexto = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain(TesteErroController.MensagemInternaSensivel, corpoTexto);
        Assert.DoesNotContain("Ocorreu um erro ao processar a requisição.", corpoTexto);
    }

    private static async Task AssertarRespostaGenericaSemVazamentoAsync(HttpResponseMessage resposta)
    {
        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);

        var corpoTexto = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain(TesteErroController.MensagemInternaSensivel, corpoTexto);
        Assert.DoesNotContain(nameof(InvalidOperationException), corpoTexto);
        Assert.DoesNotContain("   at ", corpoTexto); // assinatura de stack trace serializado

        var corpo = await resposta.Content.ReadFromJsonAsync<ErroRespostaDto>();
        Assert.NotNull(corpo);
        Assert.Single(corpo!.Erros);
        Assert.Equal("Ocorreu um erro ao processar a requisição.", corpo.Erros[0]);
    }

    private static WebApplicationFactory<Program> CriarFactoryComControllerDeErro(string? ambiente = null)
    {
        // A factory base (ProvaApiFactory) fica de pé propositalmente: o
        // WebApplicationFactory derivado por WithWebHostBuilder depende dela
        // internamente (delegates de CreateServer/CreateHost ligados à
        // instância original). Dispor dela aqui derrubaria o host derivado
        // antes mesmo do teste rodar. O teste dispõe apenas a factory
        // derivada, que é a única exposta e usada para criar o HttpClient.
        var factoryBase = new ProvaApiFactory();

        return factoryBase.WithWebHostBuilder(builder =>
        {
            if (ambiente is not null)
            {
                builder.UseEnvironment(ambiente);
            }

            builder.ConfigureServices(services =>
            {
                services.AddControllers().AddApplicationPart(typeof(TesteErroController).Assembly);
            });
        });
    }

    /// <summary>
    /// Insere, DEPOIS de todo o pipeline real de <c>Program.cs</c> (que
    /// começa com <see cref="ExceptionHandlingMiddleware"/> e termina com
    /// <c>MapControllers()</c>), um middleware terminal que escreve um
    /// caractere na resposta e lança em seguida. Precisa vir depois de
    /// <c>next(app)</c> — e não antes — para que a exceção seja lançada
    /// "dentro" do try/catch do <see cref="ExceptionHandlingMiddleware"/>
    /// (que é o primeiro middleware do pipeline real) e não antes dele, onde
    /// nada a capturaria. Como nenhuma rota de controller casa com
    /// <see cref="Caminho"/>, o routing deixa a requisição cair neste
    /// middleware — único jeito de simular "exceção depois que a resposta já
    /// começou a ser enviada" via teste de integração, sem alterar o
    /// middleware de produção.
    /// </summary>
    private sealed class RespostaJaIniciadaStartupFilter : IStartupFilter
    {
        public const string Caminho = "/api/teste-erro/resposta-ja-iniciada";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                next(app);

                app.Use(async (context, nextMiddleware) =>
                {
                    if (context.Request.Path == Caminho)
                    {
                        await context.Response.WriteAsync(" ");
                        throw new InvalidOperationException(TesteErroController.MensagemInternaSensivel);
                    }

                    await nextMiddleware();
                });
            };
        }
    }
}
