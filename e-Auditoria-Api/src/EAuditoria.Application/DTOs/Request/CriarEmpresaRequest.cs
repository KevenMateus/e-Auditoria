using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Request;

/// <summary>Dados para cadastro de uma nova empresa.</summary>
public class CriarEmpresaRequest
{
    /// <summary>Razão social completa da empresa.</summary>
    /// <example>Contabilidade ABC Ltda</example>
    [Required]
    [MaxLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ da empresa, somente dígitos (14 caracteres).
    /// Deve ser único no sistema.
    /// </summary>
    /// <example>12345678000199</example>
    [Required]
    [Length(14, 14)]
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>
    /// Regime tributário da empresa. Define automaticamente quais obrigações fiscais
    /// são geradas (DAS, DCTF, EFD, eSocial, SPED etc.).
    /// </summary>
    /// <example>SimplesNacional</example>
    [Required]
    [DefaultValue(RegimeTributario.SimplesNacional)]
    public RegimeTributario RegimeTributario { get; set; }
}
