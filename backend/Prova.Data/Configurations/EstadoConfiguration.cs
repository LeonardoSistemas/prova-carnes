using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prova.Model.Entities;

namespace Prova.Data.Configurations;

/// <summary>
/// Mapeamento de <see cref="Estado"/>. Entidade de apoio, somente leitura
/// para o usuário final — dados semeados via <see cref="Seed.EstadoCidadeSeed"/>.
/// </summary>
public class EstadoConfiguration : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        builder.ToTable("Estados");

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Uf)
            .IsRequired()
            .HasMaxLength(2);

        builder.HasData(Seed.EstadoCidadeSeed.Estados);
    }
}
