using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Controllers;

/// <summary>
/// Calendário de obrigações acessórias por empresa e competência.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Obrigações")]
public class ObrigacoesController : ControllerBase
{
    private readonly IObrigacaoService _obrigacaoService;

    public ObrigacoesController(IObrigacaoService obrigacaoService)
    {
        _obrigacaoService = obrigacaoService;
    }

    /// <summary>
    /// Retorna o calendário de obrigações de uma empresa para um mês/ano.
    /// </summary>
    /// <remarks>
    /// O status é recalculado em tempo real com base na data atual.
    /// Use o parâmetro <c>status</c> para filtrar por um status específico.
    ///
    ///     GET /api/obrigacoes/calendario?empresaId={id}&amp;mes=6&amp;ano=2025
    ///     GET /api/obrigacoes/calendario?empresaId={id}&amp;mes=6&amp;ano=2025&amp;status=Atrasada
    ///
    /// </remarks>
    /// <param name="empresaId">UUID da empresa.</param>
    /// <param name="mes">Mês de competência (1–12).</param>
    /// <param name="ano">Ano de competência (ex: 2025).</param>
    /// <param name="status">Filtro opcional de status: Pendente | Atrasada | Entregue | NaoAplicavel</param>
    /// <returns>Lista de obrigações do período.</returns>
    /// <response code="200">Calendário retornado. Pode ser vazio se nenhuma obrigação foi gerada ainda.</response>
    /// <response code="400">Parâmetros inválidos (mês fora do intervalo, empresa sem UUID válido).</response>
    [HttpGet("calendario")]
    [ProducesResponseType(typeof(IEnumerable<ObrigacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ObrigacaoResponse>>> Calendario(
        [FromQuery] Guid empresaId,
        [FromQuery] int mes,
        [FromQuery] int ano,
        [FromQuery] StatusObrigacao? status = null)
    {
        if (mes < 1 || mes > 12)
            return BadRequest(new { mensagem = "O parâmetro 'mes' deve estar entre 1 e 12." });

        var obrigacoes = await _obrigacaoService.ObterCalendarioAsync(empresaId, mes, ano, status);
        return Ok(obrigacoes);
    }

    /// <summary>
    /// Gera as obrigações devidas para uma empresa em um mês/ano.
    /// </summary>
    /// <remarks>
    /// A operação é **idempotente**: obrigações já existentes não são duplicadas.
    /// A engine calcula quais obrigações se aplicam ao regime tributário da empresa
    /// e os vencimentos conforme o calendário fiscal.
    ///
    ///     POST /api/obrigacoes/gerar
    ///     {
    ///         "empresaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "mes": 6,
    ///         "ano": 2025
    ///     }
    ///
    /// **Regras de vencimento aplicadas:**
    /// - DAS: dia 20 do mês seguinte (prorrogado para dia útil se fim de semana)
    /// - DCTF: dia 15 do 2º mês seguinte
    /// - EFD-ICMS/IPI, EFD Contribuições, EFD-Reinf: dia 15 do mês seguinte
    /// - eSocial: dia 7 do mês seguinte
    /// - SPED ECD: 31/05 do ano seguinte (gerado em janeiro)
    /// - SPED ECF: 31/07 do ano seguinte (gerado em janeiro)
    /// - DIRF: último dia de fevereiro do ano seguinte (gerado em janeiro)
    /// - RAIS / DEFIS: 31/03 do ano seguinte (gerado em janeiro)
    /// </remarks>
    /// <param name="request">Empresa, mês e ano de competência.</param>
    /// <returns>Lista de obrigações geradas (ou já existentes) para o período.</returns>
    /// <response code="200">Obrigações geradas ou confirmadas como já existentes.</response>
    /// <response code="404">Empresa não encontrada.</response>
    [HttpPost("gerar")]
    [ProducesResponseType(typeof(IEnumerable<ObrigacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ObrigacaoResponse>>> Gerar([FromBody] GerarObrigacoesRequest request)
    {
        var obrigacoes = await _obrigacaoService.GerarObrigacoesAsync(request);
        return Ok(obrigacoes);
    }

    /// <summary>
    /// Exporta o calendário de obrigações de uma empresa em formato CSV.
    /// </summary>
    /// <remarks>
    /// Retorna um arquivo CSV com BOM UTF-8 (compatível com Excel) contendo todas as
    /// obrigações da empresa no período informado.
    ///
    ///     GET /api/obrigacoes/exportar?empresaId={id}&amp;mes=6&amp;ano=2025
    ///
    /// </remarks>
    /// <param name="empresaId">UUID da empresa.</param>
    /// <param name="mes">Mês de competência (1–12).</param>
    /// <param name="ano">Ano de competência.</param>
    /// <response code="200">Arquivo CSV gerado.</response>
    /// <response code="404">Empresa não encontrada.</response>
    [HttpGet("exportar")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportarCsv(
        [FromQuery] Guid empresaId,
        [FromQuery] int mes,
        [FromQuery] int ano)
    {
        var csv = await _obrigacaoService.ExportarCsvAsync(empresaId, mes, ano);
        var nomeArquivo = $"obrigacoes_{mes:D2}_{ano}.csv";
        return File(csv, "text/csv; charset=utf-8", nomeArquivo);
    }

    /// <summary>
    /// Obtém uma obrigação pelo Id.
    /// </summary>
    /// <param name="id">UUID da obrigação.</param>
    /// <returns>Dados da obrigação, incluindo entrega se houver.</returns>
    /// <response code="200">Obrigação encontrada.</response>
    /// <response code="404">Obrigação não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ObrigacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ObrigacaoResponse>> ObterPorId(Guid id)
    {
        var obrigacao = await _obrigacaoService.ObterPorIdAsync(id);
        return obrigacao is null ? NotFound() : Ok(obrigacao);
    }
}
