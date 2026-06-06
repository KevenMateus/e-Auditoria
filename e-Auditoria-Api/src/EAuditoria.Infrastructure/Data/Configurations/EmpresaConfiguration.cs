using EAuditoria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EAuditoria.Infrastructure.Data.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.RazaoSocial)
            .HasColumnName("razao_social")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Cnpj)
            .HasColumnName("cnpj")
            .HasMaxLength(14)
            .IsRequired();

        builder.HasIndex(e => e.Cnpj)
            .IsUnique()
            .HasDatabaseName("ix_empresas_cnpj");

        builder.Property(e => e.RegimeTributario)
            .HasColumnName("regime_tributario")
            .IsRequired();

        builder.Property(e => e.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true);

        builder.Property(e => e.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(e => e.AtualizadoEm)
            .HasColumnName("atualizado_em");

        builder.HasMany(e => e.Obrigacoes)
            .WithOne(o => o.Empresa)
            .HasForeignKey(o => o.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
