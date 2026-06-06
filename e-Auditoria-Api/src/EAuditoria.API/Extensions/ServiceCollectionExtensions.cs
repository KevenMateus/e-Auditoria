using EAuditoria.API.Dependencies;

namespace EAuditoria.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddInfrastructureServices(configuration)
            .AddRepositories()
            .AddJwtAuth(configuration);

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddServices();
        return services;
    }
}
