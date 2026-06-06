using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Controllers;

/// <summary>
/// Registro de entregas de obrigações e histórico por empresa.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Entregas")]
public class EntregasController : ControllerBase
{
    private readonly IEntregaService _entregaService;

    public EntregasController(IEntregaService entregaService)
    {
        _entregaService = entregaService;
    }

    /// <summary>
    /// Registra uma obrigação como entregue.
    /// </summary>
    /// <remarks>
    /// Ao registrar a entrega, o status da obrigação muda para <c>Entregue</c>.
    /// A data de entrega pode ser retroativa (é a data efetiva de cumprimento, não a de registro no sistema).
    ///
    ///     POST /api/entregas/obrigacoes/{obrigacaoId}
    ///     {
    ///         "dataEntrega": "2025-06-10T00:00:00Z",
    ///         "observacao": "Entregue via sistema SPED"
    ///     }
    ///
    /// </remarks>
    /// <param name="obrigacaoId">UUID da obrigação a marcar como entregue.</param>
    /// <param name="request">Data de entrega e observação opcional.</param>
    /// <returns>Registro de entrega criado.</returns>
    /// <response code="201">Entrega registrada com sucesso.</response>
    /// <response code="404">Obrigação não encontrada.</response>
    /// <response code="422">Obrigação já foi registrada como entregue.</response>
    [HttpPost("obrigacoes/{obrigacaoId:guid}")]
    [ProducesResponseType(typeof(EntregaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EntregaResponse>> Registrar(
        Guid obrigacaoId,
        [FromBody] RegistrarEntregaRequest request)
    {
        var entrega = await _entregaService.RegistrarAsync(obrigacaoId, request);
        return CreatedAtAction(nameof(Historico), new { empresaId = entrega.ObrigacaoId }, entrega);
    }

    /// <summary>
    /// Retorna o histórico de entregas de uma empresa.
    /// </summary>
    /// <remarks>
    /// Lista todas as entregas registradas para as obrigações da empresa,
    /// ordenadas da mais recente para a mais antiga.
    ///
    ///     GET /api/entregas/historico/{empresaId}
    ///
    /// </remarks>
    /// <param name="empresaId">UUID da empresa.</param>
    /// <returns>Lista de entregas da empresa, ordenada por data decrescente.</returns>
    /// <response code="200">Histórico retornado (pode ser vazio se não houver entregas).</response>
    [HttpGet("historico/{empresaId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<EntregaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EntregaResponse>>> Historico(Guid empresaId)
    {
        var historico = await _entregaService.ObterHistoricoAsync(empresaId);
        return Ok(historico);
    }
}
