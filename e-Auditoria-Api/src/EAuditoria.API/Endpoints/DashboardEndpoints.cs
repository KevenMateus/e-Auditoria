using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;

namespace EAuditoria.API.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (int? mes, int? ano, IDashboardService service) =>
        {
            var mesRef = mes ?? DateTime.UtcNow.Month;
            var anoRef = ano ?? DateTime.UtcNow.Year;
            var dashboard = await service.ObterAsync(mesRef, anoRef);
            return Results.Ok(dashboard);
        })
        .WithTags("Dashboard")
        .WithSummary("Retorna os indicadores consolidados para um mês/ano.")
        .WithDescription("""
            Visão consolidada do painel: total de empresas, total de obrigações do mês e
            contagem por status (Pendentes, Entregues, Atrasadas).

            Se `mes` e `ano` não forem informados, usa o mês/ano corrente.
            """)
        .Produces<DashboardResponse>(StatusCodes.Status200OK);

        group.MapGet("/alertas", async (IDashboardService service) =>
        {
            var alertas = await service.ObterAlertasAsync();
            return Results.Ok(alertas);
        })
        .WithTags("Dashboard")
        .WithSummary("Lista alertas de obrigações vencendo em 30 dias ou já atrasadas.")
        .WithDescription("""
            Retorna obrigações com vencimento nos próximos 30 dias (`Pendente`) e as já vencidas sem entrega (`Atrasada`),
            ordenadas por urgência (mais atrasadas primeiro, depois as que vencem mais cedo).

            `DiasRestantes` negativo indica dias de atraso.
            """)
        .Produces<IEnumerable<AlertaObrigacaoResponse>>(StatusCodes.Status200OK);

        return group;
    }
}
