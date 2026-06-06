using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EAuditoria.API.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IAuthService authService,
            ILogger<Program> logger) =>
        {
            try
            {
                var response = await authService.LoginAsync(request);
                logger.LogInformation("Login realizado: {Email}", request.Email);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("Tentativa de login falhou para {Email}: {Message}", request.Email, ex.Message);
                return Results.Problem(
                    title: "Acesso negado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .WithTags("Auth")
        .WithSummary("Realiza login e retorna um token JWT.")
        .WithDescription("""
            Autentica o usuário com e-mail e senha.

            Retorna um token JWT que deve ser incluído em todas as demais requisições
            no header `Authorization: Bearer {token}`.

            **Credenciais padrão do seed:**
            - Admin: `admin@eauditoria.com.br` / `admin123`
            - Operador: `operador@eauditoria.com.br` / `operador123`
            """)
        .AllowAnonymous()
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", (HttpContext ctx) =>
        {
            var user = ctx.User;
            var sub    = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst("sub")?.Value;
            var email  = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                      ?? user.FindFirst("email")?.Value;
            var nome   = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                      ?? user.FindFirst("name")?.Value;
            var perfil = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            return Results.Ok(new { id = sub, email, nome, perfil });
        })
        .WithTags("Auth")
        .WithSummary("Retorna dados do usuário autenticado.")
        .WithDescription("Decodifica o token JWT do header e retorna id, e-mail, nome e perfil do usuário corrente.")
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return group;
    }
}
