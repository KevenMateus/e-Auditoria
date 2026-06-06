using EAuditoria.Application.Interfaces.Repositories;
using EAuditoria.Domain.Interfaces;
using EAuditoria.Infrastructure.Repositories;

namespace EAuditoria.API.Dependencies;

public static class RepositoryDependencies
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IObrigacaoRepository, ObrigacaoRepository>();
        services.AddScoped<IEntregaRepository, EntregaRepository>();

        return services;
    }
}
