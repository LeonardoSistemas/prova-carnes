using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Prova.Data.Context;

/// <summary>
/// Factory usada pelas ferramentas de design-time (`dotnet ef ...`).
///
/// Importante: o EF Core SEMPRE prefere uma implementação de
/// <see cref="IDesignTimeDbContextFactory{TContext}"/> encontrada no projeto,
/// mesmo quando `dotnet ef` é executado a partir de `Prova.Api` com
/// `--startup-project` apontando pra lá — ou seja, esta classe é quem decide
/// a connection string usada por `dotnet ef migrations add`/`database update`,
/// não o `Program.cs`/`AddDbContext` da Api (essa distinção já causou
/// divergência entre o que o README documentava e o comportamento real).
/// Por isso ela lê a MESMA connection string de `Prova.Api/appsettings.json`
/// (`ConnectionStrings:DefaultConnection`) em vez de um valor hardcoded — design-time
/// e runtime sempre usam o mesmo banco, sem exigir LocalDB especificamente.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // `dotnet ef` redireciona o cwd desta factory para o diretório do
        // `--project` resolvido (comportamento da própria ferramenta, não
        // deste código) — por isso checar `appsettings.json` no cwd atual
        // cobre tanto rodar de dentro de Prova.Api quanto de Prova.Data
        // (com --startup-project ../Prova.Api). Se a factory for invocada
        // fora do fluxo `dotnet ef` (ex.: instanciada direto em outro
        // contexto) e nenhum dos dois caminhos existir, `AddJsonFile(...,
        // optional: false)` abaixo lança com o caminho tentado na mensagem —
        // não falha silenciosamente.
        var basePath = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            basePath = Path.Combine(basePath, "..", "Prova.Api");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                $"Connection string 'DefaultConnection' não encontrada em '{Path.Combine(basePath, "appsettings.json")}'.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
