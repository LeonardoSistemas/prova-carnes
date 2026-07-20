using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prova.Model.Entities;

namespace Prova.Data.Configurations;

/// <summary>
/// Mapeamento de <see cref="Carne"/>. Origem é mapeada como int (padrão do
/// EF Core para enum) — não há necessidade de conversão para string nesta
/// prova.
/// </summary>
public class CarneConfiguration : IEntityTypeConfiguration<Carne>
{
    public void Configure(EntityTypeBuilder<Carne> builder)
    {
        builder.ToTable("Carnes");

        builder.Property(c => c.Descricao)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Origem)
            .IsRequired();
    }
}
