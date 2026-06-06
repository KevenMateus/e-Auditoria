using EAuditoria.Domain.Entities;
using EAuditoria.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<ObrigacaoAcessoria> ObrigacoesAcessorias => Set<ObrigacaoAcessoria>();
    public DbSet<EntregaObrigacao> EntregasObrigacoes => Set<EntregaObrigacao>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new EmpresaConfiguration());
        modelBuilder.ApplyConfiguration(new ObrigacaoAcessoriaConfiguration());
        modelBuilder.ApplyConfiguration(new EntregaObrigacaoConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
    }
}
