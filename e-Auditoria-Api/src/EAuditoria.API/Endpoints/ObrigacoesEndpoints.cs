using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Endpoints;

public static class ObrigacoesEndpoints
{
    public static RouteGroupBuilder MapObrigacoesEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/calendario", async (
            [FromQuery] Guid empresaId,
            [FromQuery] int mes,
            [FromQuery] int ano,
            [FromQuery] string? status,
            IObrigacaoService service) =>
        {
            if (mes < 1 || mes > 12)
                return Results.BadRequest(new { mensagem = "O parâmetro 'mes' deve estar entre 1 e 12." });

            StatusObrigacao? filtroStatus = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<StatusObrigacao>(status, ignoreCase: true, out var parsed))
                filtroStatus = parsed;

            var obrigacoes = await service.ObterCalendarioAsync(empresaId, mes, ano, filtroStatus);
            return Results.Ok(obrigacoes);
        })
        .WithTags("Obrigações")
        .WithSummary("Retorna o calendário de obrigações de uma empresa para um mês/ano.")
        .WithDescription("""
            Lista todas as obrigações fiscais de uma empresa para a competência informada.

            Filtre por `status` para ver apenas obrigações `Pendente`, `Atrasada` ou `Entregue`.
            Obrigações anuais (SPED ECD, SPED ECF, DIRF, RAIS, DEFIS) aparecem apenas no mês de janeiro.
            """)
        .Produces<IEnumerable<ObrigacaoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/gerar", async ([FromBody] GerarObrigacoesRequest request, IObrigacaoService service) =>
        {
            var obrigacoes = await service.GerarObrigacoesAsync(request);
            return Results.Ok(obrigacoes);
        })
        .WithTags("Obrigações")
        .WithSummary("Gera as obrigações devidas para uma empresa em um mês/ano.")
        .WithDescription("""
            Executa a engine de regras tributárias e persiste as obrigações para a competência informada.

            Idempotente: se as obrigações já existem para aquela competência, retorna as existentes sem duplicar.
            O conjunto de obrigações geradas depende do regime tributário da empresa.
            """)
        .Produces<IEnumerable<ObrigacaoResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/exportar", async (
            [FromQuery] Guid empresaId,
            [FromQuery] int mes,
            [FromQuery] int ano,
            IObrigacaoService service) =>
        {
            var csv = await service.ExportarCsvAsync(empresaId, mes, ano);
            var nomeArquivo = $"obrigacoes_{mes:D2}_{ano}.csv";
            return Results.File(csv, "text/csv; charset=utf-8", nomeArquivo);
        })
        .WithTags("Obrigações")
        .WithSummary("Exporta o calendário de obrigações em formato CSV.")
        .WithDescription("""
            Retorna um arquivo `.csv` com todas as obrigações da empresa para o mês/ano informado.

            Colunas: Obrigação, Periodicidade, Competência, Vencimento, Status, Data de Entrega, Observação.
            Encoding: UTF-8.
            """)
        .Produces<byte[]>(StatusCodes.Status200OK, "text/csv")
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", async (Guid id, IObrigacaoService service) =>
        {
            var obrigacao = await service.ObterPorIdAsync(id);
            return obrigacao is null ? Results.NotFound() : Results.Ok(obrigacao);
        })
        .WithTags("Obrigações")
        .WithSummary("Obtém uma obrigação pelo Id.")
        .WithDescription("Retorna os dados completos de uma obrigação, incluindo entrega (se houver).")
        .Produces<ObrigacaoResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
