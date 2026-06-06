using EAuditoria.Infrastructure.Data.Seed;

namespace EAuditoria.API.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/seed", async (DatabaseSeeder seeder, ILogger<Program> logger) =>
        {
            logger.LogInformation("Seed manual acionado via API.");
            await seeder.SeedAsync();
            return Results.Ok(new { mensagem = "Seed executado com sucesso." });
        })
        .WithTags("Admin")
        .WithSummary("Executa o seed de dados de demonstração.")
        .WithDescription("""
            Popula o banco com dados de demonstração: empresas de todos os regimes tributários,
            usuários padrão (Admin e Operador) e obrigações geradas para os últimos 3 meses.

            **Requer perfil Admin.** Idempotente: re-executar não duplica dados existentes.
            """)
        .RequireAuthorization("Admin")
        .Produces(StatusCodes.Status200OK);

        return group;
    }
}
