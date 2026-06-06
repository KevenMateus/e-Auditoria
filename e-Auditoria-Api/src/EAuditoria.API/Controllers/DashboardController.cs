using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Controllers;

/// <summary>
/// Dashboard consolidado e painel de alertas.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Retorna os indicadores consolidados para um mês/ano.
    /// </summary>
    /// <remarks>
    /// Se <c>mes</c> e <c>ano</c> não forem informados, usa o mês e ano correntes (UTC).
    ///
    ///     GET /api/dashboard
    ///     GET /api/dashboard?mes=6&amp;ano=2025
    ///
    /// **Campos retornados:**
    /// - `totalEmpresas`: total de empresas ativas no sistema
    /// - `obrigacoesMes`: total de obrigações geradas para o mês/ano
    /// - `pendentes`: obrigações com vencimento futuro, não entregues
    /// - `entregues`: obrigações com entrega registrada
    /// - `atrasadas`: obrigações com vencimento passado, não entregues
    /// </remarks>
    /// <param name="mes">Mês de referência (1–12). Padrão: mês atual.</param>
    /// <param name="ano">Ano de referência. Padrão: ano atual.</param>
    /// <returns>Contadores do dashboard.</returns>
    /// <response code="200">Dashboard retornado com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardResponse>> Obter(
        [FromQuery] int? mes = null,
        [FromQuery] int? ano = null)
    {
        var mesRef = mes ?? DateTime.UtcNow.Month;
        var anoRef = ano ?? DateTime.UtcNow.Year;

        var dashboard = await _dashboardService.ObterAsync(mesRef, anoRef);
        return Ok(dashboard);
    }

    /// <summary>
    /// Lista obrigações vencendo em 30 dias e as já atrasadas, ordenadas por urgência.
    /// </summary>
    /// <remarks>
    /// Retorna uma lista unificada com obrigações atrasadas (diasRestantes negativo)
    /// e as que vencem nos próximos 30 dias, ordenadas do mais urgente para o menos urgente.
    ///
    ///     GET /api/dashboard/alertas
    ///
    /// **Interpretação de `diasRestantes`:**
    /// - Valor negativo: quantidade de dias em atraso (ex: `-3` = 3 dias atrasada)
    /// - `0`: vence hoje
    /// - Valor positivo: dias até o vencimento (ex: `7` = vence em 7 dias)
    /// </remarks>
    /// <returns>Lista de alertas ordenada por urgência (mais urgente primeiro).</returns>
    /// <response code="200">Lista de alertas retornada (pode ser vazia).</response>
    [HttpGet("alertas")]
    [ProducesResponseType(typeof(IEnumerable<AlertaObrigacaoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertaObrigacaoResponse>>> Alertas()
    {
        var alertas = await _dashboardService.ObterAlertasAsync();
        return Ok(alertas);
    }
}
