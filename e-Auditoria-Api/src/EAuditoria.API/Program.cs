using EAuditoria.API.Dependencies;
using EAuditoria.API.Endpoints;
using EAuditoria.API.Extensions;
using EAuditoria.API.Middleware;
using EAuditoria.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
    opts.SerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddSwaggerDocumentation()
    .AddCorsPolicy();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<EAuditoria.Infrastructure.Data.AppDbContext>();

        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            Log.Information("Aplicando {Count} migration(s) pendente(s)...", pendingMigrations.Count());
            await db.Database.MigrateAsync();
        }
        else
        {
            var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
            if (!appliedMigrations.Any())
            {
                Log.Warning("Nenhuma migration encontrada no assembly. Criando schema via EnsureCreated.");
                await db.Database.EnsureCreatedAsync();
            }
            else
            {
                Log.Information("Banco de dados já atualizado ({Count} migration(s) aplicada(s)).", appliedMigrations.Count());
            }
        }

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Falha ao inicializar o banco de dados.");
        throw;
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwaggerDocumentation();
app.UseCors(CorsDependencies.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api/auth").MapAuthEndpoints();

app.MapGroup("/api/empresas").RequireAuthorization().MapEmpresasEndpoints();
app.MapGroup("/api/obrigacoes").RequireAuthorization().MapObrigacoesEndpoints();
app.MapGroup("/api/entregas").RequireAuthorization().MapEntregasEndpoints();
app.MapGroup("/api/dashboard").RequireAuthorization().MapDashboardEndpoints();
app.MapGroup("/api/admin").RequireAuthorization("Admin").MapAdminEndpoints();

app.Run();
