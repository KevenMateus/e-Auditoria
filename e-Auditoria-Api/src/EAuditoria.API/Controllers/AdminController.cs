using EAuditoria.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Controllers;

/// <summary>
/// Operações administrativas (seed, diagnóstico).
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Admin")]
public class AdminController : ControllerBase
{
    private readonly DatabaseSeeder _seeder;
    private readonly ILogger<AdminController> _logger;

    public AdminController(DatabaseSeeder seeder, ILogger<AdminController> logger)
    {
        _seeder = seeder;
        _logger = logger;
    }

    /// <summary>
    /// Executa o seed de dados de demonstração.
    /// </summary>
    /// <remarks>
    /// Idempotente: cria empresas e obrigações apenas se não existirem.
    /// Use para popular o banco na primeira execução ou recuperar após reset.
    /// </remarks>
    /// <response code="200">Seed executado com sucesso.</response>
    [HttpPost("seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Seed()
    {
        _logger.LogInformation("Seed manual acionado via API.");
        await _seeder.SeedAsync();
        return Ok(new { mensagem = "Seed executado com sucesso." });
    }
}
