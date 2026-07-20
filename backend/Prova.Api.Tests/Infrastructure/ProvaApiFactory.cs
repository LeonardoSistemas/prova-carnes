using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prova.Data.Context;
using Prova.Model.Entities;
using Prova.Service.Cotacao;

namespace Prova.Api.Tests.Infrastructure;

/// <summary>
/// <see cref="WebApplicationFactory{Program}"/> customizada para os testes de
/// integração: substitui o <see cref="AppDbContext"/> real (SQL Server) por
/// um banco em memória do próprio EF Core (provider InMemory), isolado por
/// instância (nome de banco único por factory), e permite trocar o
/// <see cref="ICotacaoService"/> real (que chamaria a AwesomeAPI de verdade)
/// por um dublê de teste (<see cref="CotacaoServiceFake"/>).
///
/// Decisão de provider (EF Core InMemory, não SQLite in-memory): o provider
/// InMemory não valida FK/constraint como um banco relacional de verdade, mas
/// a regra de bloqueio de delete por FK (Carne↔PedidoItem,
/// Comprador↔Pedido) já tem cobertura de teste unitário na camada Service
/// (com mock do repositório) — o teste de integração aqui existe para
/// validar o fluxo HTTP + status code (200/201/404/409/422), não a regra de
/// negócio em si, então a fidelidade extra de FK do SQLite não agrega valor
/// suficiente para justificar a complexidade adicional de setup.
/// </summary>
public class ProvaApiFactory : WebApplicationFactory<Program>
{
    private readonly string _nomeBanco = Guid.NewGuid().ToString();
    private readonly ICotacaoService _cotacaoService;

    public ProvaApiFactory(ICotacaoService? cotacaoService = null)
    {
        _cotacaoService = cotacaoService ?? CotacaoServiceFake.ComSucesso();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_nomeBanco));

            services.RemoveAll<ICotacaoService>();
            services.AddScoped(_ => _cotacaoService);
        });
    }

    /// <summary>
    /// Semeia Estado/Cidade diretamente no banco de teste (sem passar pela
    /// API — não existe endpoint de escrita de Estado/Cidade, é dado somente
    /// leitura por decisão de produto/T15) para viabilizar a criação de
    /// Comprador nos testes que dependem de um <c>CidadeId</c> válido.
    /// </summary>
    public async Task<Cidade> SemearCidadeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var estado = new Estado { Nome = "São Paulo", Uf = "SP" };
        var cidade = new Cidade { Nome = "São Paulo", Estado = estado };

        context.Cidades.Add(cidade);
        await context.SaveChangesAsync();

        return cidade;
    }
}
