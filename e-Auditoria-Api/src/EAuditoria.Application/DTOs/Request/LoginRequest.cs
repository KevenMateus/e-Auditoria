using System.ComponentModel.DataAnnotations;

namespace EAuditoria.Application.DTOs.Request;

/// <summary>Credenciais para autenticação.</summary>
public class LoginRequest
{
    /// <summary>E-mail do usuário cadastrado no sistema.</summary>
    /// <example>admin@eauditoria.com.br</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Senha do usuário (mínimo 6 caracteres).</summary>
    /// <example>admin123</example>
    [Required]
    [MinLength(6)]
    public string Senha { get; set; } = string.Empty;
}
