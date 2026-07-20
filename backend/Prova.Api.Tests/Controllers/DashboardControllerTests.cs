using System.Net;
using System.Net.Http.Json;
using Prova.Api.Middlewares;
using Prova.Api.Tests.Infrastructure;
using Prova.Service.Dtos;

namespace Prova.Api.Tests.Controllers;

/// <summary>
/// Testes de integração de <c>DashboardController</c> (T62): cobrem 200 com o
/// shape combinado de <see cref="DashboardDto"/> (T59+T60) no
/// <c>GET /api/dashboard</c>, 400 quando <c>periodo</c> é ausente/inválido, 200
/// com a série completa no <c>GET /api/dashboard/faturamento-por-dia</c> (T61)
/// e 400 quando <c>dias</c> é &lt;= 0 (mapeado pelo middleware global a partir
/// do <see cref="ArgumentOutOfRangeException"/> lançado pela Service).
///
/// Não há regra de negócio nova testável aqui além do roteamento/validação de
/// query string — o cálculo em si (ticket médio, top 5, série sem buracos)
/// já tem cobertura de teste unitário em <c>DashboardServiceTests</c> (T59-T61).
/// </summary>
public class DashboardControllerTests
{
    [Fact]
    public async Task Get_PeriodoHoje_Retorna200ComShapeCombinadoDeResumoETop()
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/dashboard?periodo=hoje");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<DashboardDto>();
        Assert.NotNull(corpo);
        Assert.NotNull(corpo!.Resumo);
        Assert.NotNull(corpo.Top);
        Assert.NotNull(corpo.Top.TopCarnes);
        Assert.NotNull(corpo.Top.TopCompradores);
    }

    [Theory]
    [InlineData("SEMANA")]
    [InlineData("Mes")]
    public async Task Get_PeriodoValidoCaseInsensitive_Retorna200(string periodo)
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync($"/api/dashboard?periodo={periodo}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Get_PeriodoInvalido_Retorna400ComMensagemClara()
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/dashboard?periodo=ano");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ErroRespostaDto>();
        Assert.NotNull(corpo);
        Assert.Contains(corpo!.Erros, m => m.Contains("Período inválido"));
    }

    [Fact]
    public async Task Get_PeriodoAusente_Retorna400()
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/dashboard");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task GetFaturamentoPorDia_Dias7_Retorna200Com7Itens()
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/dashboard/faturamento-por-dia?dias=7");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var serie = await resposta.Content.ReadFromJsonAsync<List<FaturamentoPorDiaDto>>();
        Assert.NotNull(serie);
        Assert.Equal(7, serie!.Count);
    }

    [Fact]
    public async Task GetFaturamentoPorDia_Dias30_Retorna200Com30Itens()
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/dashboard/faturamento-por-dia?dias=30");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var serie = await resposta.Content.ReadFromJsonAsync<List<FaturamentoPorDiaDto>>();
        Assert.NotNull(serie);
        Assert.Equal(30, serie!.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetFaturamentoPorDia_DiasZeroOuNegativo_Retorna400ComMensagemClara(int dias)
    {
        using var factory = new ProvaApiFactory();
        using var client = factory.CreateClient();

        var resposta = await client.GetAsync($"/api/dashboard/faturamento-por-dia?dias={dias}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpoTexto = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("   at ", corpoTexto); // nunca vaza stack trace
        // ArgumentOutOfRangeException.Message concatena automaticamente um
        // sufixo técnico cru ("(Parameter 'dias')\r\nActual value was 0.")
        // -- o middleware não pode repassar isso ao cliente.
        Assert.DoesNotContain("Actual value was", corpoTexto);
        Assert.DoesNotContain("Parameter '", corpoTexto);

        var corpo = await resposta.Content.ReadFromJsonAsync<ErroRespostaDto>();
        Assert.NotNull(corpo);
        Assert.NotEmpty(corpo!.Erros);
    }
}
