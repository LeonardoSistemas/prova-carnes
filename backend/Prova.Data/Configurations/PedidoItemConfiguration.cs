using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prova.Model.Entities;

namespace Prova.Data.Configurations;

/// <summary>
/// Mapeamento de <see cref="PedidoItem"/>, entidade associativa entre Pedido
/// e Carne.
/// - PedidoItem → Pedido: Cascade. É a única cascade esperada no modelo —
///   ao excluir um Pedido, os itens somem junto (consistente com a task de
///   DELETE /pedidos/{id} retornando 204 e removendo itens em cascade).
/// - PedidoItem → Carne: Restrict. Reforça a nível de banco a regra de
///   negócio da Service que bloqueia exclusão de Carne com PedidoItem
///   vinculado.
/// <see cref="PedidoItem.ValorEmReal"/> é propriedade calculada e nunca deve
/// ser mapeada como coluna.
/// </summary>
public class PedidoItemConfiguration : IEntityTypeConfiguration<PedidoItem>
{
    public void Configure(EntityTypeBuilder<PedidoItem> builder)
    {
        builder.ToTable("PedidoItens");

        builder.Property(i => i.Preco)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.CotacaoUsada)
            .IsRequired()
            .HasColumnType("decimal(18,6)");

        builder.Property(i => i.Moeda)
            .IsRequired();

        builder.HasOne(i => i.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(i => i.PedidoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Carne)
            .WithMany(c => c.PedidoItens)
            .HasForeignKey(i => i.CarneId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(i => i.ValorEmReal);
    }
}
