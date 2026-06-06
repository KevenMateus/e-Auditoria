namespace EAuditoria.Application.DTOs.Response;

/// <summary>Resposta de autenticação com token JWT e dados do usuário.</summary>
public class AuthResponse
{
    /// <summary>Token JWT para autenticação nas demais rotas. Inclua no header: <c>Authorization: Bearer {token}</c>.</summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string Token { get; set; } = string.Empty;

    /// <summary>Tipo do token. Sempre <c>Bearer</c>.</summary>
    /// <example>Bearer</example>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>Tempo de expiração do token em segundos a partir da emissão.</summary>
    /// <example>86400</example>
    public int ExpiresInSeconds { get; set; }

    /// <summary>Nome completo do usuário autenticado.</summary>
    /// <example>Administrador</example>
    public string Nome { get; set; } = string.Empty;

    /// <summary>E-mail do usuário autenticado.</summary>
    /// <example>admin@eauditoria.com.br</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>Perfil de acesso do usuário (<c>Admin</c> ou <c>Operador</c>).</summary>
    /// <example>Admin</example>
    public string Perfil { get; set; } = string.Empty;
}
