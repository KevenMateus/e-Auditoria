using EAuditoria.Application.Engine;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Application.Mappings;
using EAuditoria.Application.Services;

namespace EAuditoria.API.Dependencies;

public static class ServiceDependencies
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(cfg =>
            cfg.AddMaps(typeof(DomainToDtoProfile).Assembly));
        services.AddSingleton<ITaxRulesEngine, TaxRulesEngine>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IObrigacaoService, ObrigacaoService>();
        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IEntregaService, EntregaService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
