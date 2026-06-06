using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Endpoints;

public static class EmpresasEndpoints
{
    public static RouteGroupBuilder MapEmpresasEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IEmpresaService service) =>
        {
            var empresas = await service.ListarAsync();
            return Results.Ok(empresas);
        })
        .WithTags("Empresas")
        .WithSummary("Lista todas as empresas ativas.")
        .WithDescription("Retorna todas as empresas cadastradas e ativas, ordenadas por razão social.")
        .Produces<IEnumerable<EmpresaResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IEmpresaService service) =>
        {
            var empresa = await service.ObterPorIdAsync(id);
            return empresa is null ? Results.NotFound() : Results.Ok(empresa);
        })
        .WithTags("Empresas")
        .WithSummary("Obtém uma empresa pelo Id.")
        .WithDescription("Retorna os dados completos de uma empresa a partir do seu identificador único.")
        .Produces<EmpresaResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async ([FromBody] CriarEmpresaRequest request, IEmpresaService service) =>
        {
            var empresa = await service.CriarAsync(request);
            return Results.Created($"/api/empresas/{empresa.Id}", empresa);
        })
        .WithTags("Empresas")
        .WithSummary("Cadastra uma nova empresa.")
        .WithDescription("""
            Cadastra uma empresa com razão social, CNPJ e regime tributário.

            O regime tributário define automaticamente quais obrigações fiscais serão geradas
            ao acionar o calendário (DAS, DCTF, EFD, eSocial, SPED, DIRF, RAIS etc.).

            O CNPJ deve ser único no sistema (somente dígitos, 14 caracteres).
            """)
        .Produces<EmpresaResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] AtualizarEmpresaRequest request, IEmpresaService service) =>
        {
            var empresa = await service.AtualizarAsync(id, request);
            return Results.Ok(empresa);
        })
        .WithTags("Empresas")
        .WithSummary("Atualiza razão social e/ou regime tributário.")
        .WithDescription("""
            Atualiza a razão social e o regime tributário de uma empresa existente.

            A alteração de regime passa a valer nas próximas gerações de obrigações;
            obrigações já geradas não são recalculadas retroativamente.
            """)
        .Produces<EmpresaResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IEmpresaService service) =>
        {
            await service.RemoverAsync(id);
            return Results.NoContent();
        })
        .WithTags("Empresas")
        .WithSummary("Desativa uma empresa.")
        .WithDescription("""
            Realiza exclusão lógica da empresa (marca `Ativo = false`).
            A empresa e seu histórico de obrigações são preservados no banco de dados.
            """)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}