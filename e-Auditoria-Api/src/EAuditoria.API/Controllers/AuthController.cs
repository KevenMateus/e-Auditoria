using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Controllers;

/// <summary>
/// Autenticação de usuários via JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza login e retorna um token JWT.
    /// </summary>
    /// <remarks>
    /// Usuário padrão de demonstração:
    /// - Email: admin@eauditoria.com.br
    /// - Senha: Admin@2025
    /// </remarks>
    /// <param name="request">Credenciais de acesso.</param>
    /// <returns>Token JWT e informações do usuário.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            _logger.LogInformation("Login realizado: {Email}", request.Email);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Tentativa de login falhou para {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Title  = "Acesso negado",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
            });
        }
    }

    /// <summary>
    /// Retorna informações do usuário autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var sub   = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? User.FindFirst("email")?.Value;
        var nome  = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                 ?? User.FindFirst("name")?.Value;
        var perfil = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new { id = sub, email, nome, perfil });
    }
}
