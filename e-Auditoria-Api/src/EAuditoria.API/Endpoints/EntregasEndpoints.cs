using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Endpoints;

public static class EntregasEndpoints
{
    public static RouteGroupBuilder MapEntregasEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/obrigacoes/{obrigacaoId:guid}", async (
            Guid obrigacaoId,
            [FromBody] RegistrarEntregaRequest request,
            IEntregaService service) =>
        {
            var entrega = await service.RegistrarAsync(obrigacaoId, request);
            return Results.Created($"/api/entregas/historico/{entrega.ObrigacaoId}", entrega);
        })
        .WithTags("Entregas")
        .WithSummary("Registra uma obrigação como entregue.")
        .WithDescription("""
            Marca uma obrigação como entregue, registrando a data de conclusão e uma observação opcional.

            Após o registro, o status da obrigação é alterado para `Entregue` e ela deixa de aparecer
            no painel de alertas.

            Retorna 422 se a obrigação já foi entregue anteriormente.
            """)
        .Produces<EntregaResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/historico/{empresaId:guid}", async (Guid empresaId, IEntregaService service) =>
        {
            var historico = await service.ObterHistoricoAsync(empresaId);
            return Results.Ok(historico);
        })
        .WithTags("Entregas")
        .WithSummary("Retorna o histórico de entregas de uma empresa.")
        .WithDescription("Lista todas as entregas registradas para uma empresa, ordenadas pela data de entrega mais recente.")
        .Produces<IEnumerable<EntregaResponse>>(StatusCodes.Status200OK);

        return group;
    }
}
