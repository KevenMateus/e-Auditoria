using EAuditoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EAuditoria.Infrastructure.Data.Configurations;

public class ObrigacaoAcessoriaConfiguration : IEntityTypeConfiguration<ObrigacaoAcessoria>
{
    public void Configure(EntityTypeBuilder<ObrigacaoAcessoria> builder)
    {
        builder.ToTable("obrigacoes_acessorias");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.EmpresaId)
            .HasColumnName("empresa_id")
            .IsRequired();

        builder.Property(o => o.Tipo)
            .HasColumnName("tipo")
            .IsRequired();

        builder.Property(o => o.Periodicidade)
            .HasColumnName("periodicidade")
            .IsRequired();

        builder.Property(o => o.Competencia)
            .HasColumnName("competencia")
            .IsRequired();

        builder.Property(o => o.AnoCompetencia)
            .HasColumnName("ano_competencia")
            .IsRequired();

        builder.Property(o => o.Vencimento)
            .HasColumnName("vencimento")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(o => o.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.HasIndex(o => new { o.EmpresaId, o.Tipo, o.Competencia, o.AnoCompetencia })
            .IsUnique()
            .HasDatabaseName("ix_obrigacoes_empresa_tipo_competencia");

        builder.HasIndex(o => new { o.EmpresaId, o.Competencia, o.AnoCompetencia })
            .HasDatabaseName("ix_obrigacoes_empresa_mes_ano");

        builder.HasIndex(o => new { o.Vencimento, o.Status })
            .HasDatabaseName("ix_obrigacoes_vencimento_status");

        builder.HasOne(o => o.Empresa)
            .WithMany(e => e.Obrigacoes)
            .HasForeignKey(o => o.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Entrega)
            .WithOne(e => e.Obrigacao)
            .HasForeignKey<EntregaObrigacao>(e => e.ObrigacaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
