using EAuditoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EAuditoria.Infrastructure.Data.Configurations;

public class EntregaObrigacaoConfiguration : IEntityTypeConfiguration<EntregaObrigacao>
{
    public void Configure(EntityTypeBuilder<EntregaObrigacao> builder)
    {
        builder.ToTable("entregas_obrigacoes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.ObrigacaoId)
            .HasColumnName("obrigacao_id")
            .IsRequired();

        builder.Property(e => e.DataEntrega)
            .HasColumnName("data_entrega")
            .IsRequired();

        builder.Property(e => e.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(500);

        builder.Property(e => e.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.HasIndex(e => e.ObrigacaoId)
            .IsUnique()
            .HasDatabaseName("ix_entregas_obrigacao_id");
    }
}
