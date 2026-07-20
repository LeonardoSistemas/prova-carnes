using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prova.Model.Entities;

namespace Prova.Data.Configurations;

/// <summary>
/// Mapeamento de <see cref="Comprador"/>. FK obrigatória para Cidade, sem
/// cascade. A regra de bloqueio de delete com Pedido vinculado é aplicada na
/// camada Service — aqui garantimos apenas a integridade referencial via
/// Restrict (não cascateia, não permite exclusão de Cidade em uso).
/// </summary>
public class CompradorConfiguration : IEntityTypeConfiguration<Comprador>
{
    public void Configure(EntityTypeBuilder<Comprador> builder)
    {
        builder.ToTable("Compradores");

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Documento)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(c => c.Cidade)
            .WithMany()
            .HasForeignKey(c => c.CidadeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
