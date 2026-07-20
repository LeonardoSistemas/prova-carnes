using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prova.Model.Entities;

namespace Prova.Data.Configurations;

/// <summary>
/// Mapeamento de <see cref="Pedido"/>. FK obrigatória para Comprador, sem
/// cascade — a regra de negócio de bloqueio de delete de Comprador com
/// Pedido vinculado é da camada Service, mas o FK no banco deve ser
/// condizente com essa regra (Restrict, nunca Cascade).
/// <see cref="Pedido.ValorTotalEmReal"/> é propriedade calculada e nunca
/// deve ser mapeada como coluna.
/// </summary>
public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.Property(p => p.Data)
            .IsRequired();

        builder.HasOne(p => p.Comprador)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.CompradorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(p => p.ValorTotalEmReal);
    }
}
