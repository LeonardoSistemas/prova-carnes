using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prova.Model.Entities;

namespace Prova.Data.Configurations;

/// <summary>
/// Mapeamento de <see cref="Cidade"/>. FK obrigatória para Estado, sem
/// cascade — excluir um Estado nunca deve arrastar Cidades junto (entidades
/// de apoio, mas a regra de integridade referencial vale igual).
/// </summary>
public class CidadeConfiguration : IEntityTypeConfiguration<Cidade>
{
    public void Configure(EntityTypeBuilder<Cidade> builder)
    {
        builder.ToTable("Cidades");

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(c => c.Estado)
            .WithMany(e => e.Cidades)
            .HasForeignKey(c => c.EstadoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(Seed.EstadoCidadeSeed.Cidades);
    }
}
