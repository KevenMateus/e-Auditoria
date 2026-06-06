using EAuditoria.Infrastructure.Data;
using EAuditoria.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.API.Dependencies;

public static class InfrastructureDependencies
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' não configurada. " +
                "Verifique appsettings.json ou as variáveis de ambiente.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                       npgsql.MigrationsAssembly("EAuditoria.Infrastructure"))
                   .EnableDetailedErrors()
                   .EnableSensitiveDataLogging(
                       sensitiveDataLoggingEnabled: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                           == "Development"));

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
