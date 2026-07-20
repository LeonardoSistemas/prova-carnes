using Microsoft.EntityFrameworkCore;
using Prova.Data.Configurations;
using Prova.Model.Entities;

namespace Prova.Data.Context;

/// <summary>
/// Contexto EF Core central da aplicação. Cada entidade tem seu
/// mapeamento em uma classe <see cref="Microsoft.EntityFrameworkCore.IEntityTypeConfiguration{TEntity}"/>
/// própria (Configurations/*), aplicadas via <c>ApplyConfigurationsFromAssembly</c>
/// para manter o <see cref="OnModelCreating"/> enxuto (SRP: o DbContext não
/// precisa conhecer o detalhe de cada mapeamento).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Estado> Estados => Set<Estado>();

    public DbSet<Cidade> Cidades => Set<Cidade>();

    public DbSet<Carne> Carnes => Set<Carne>();

    public DbSet<Comprador> Compradores => Set<Comprador>();

    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
