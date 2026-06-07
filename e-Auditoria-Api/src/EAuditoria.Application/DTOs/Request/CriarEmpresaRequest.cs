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
    /// CNPJ da empresa. Aceita o novo formato alfanumérico da Receita Federal
    /// (ex: AB.CDE.FGH/0001-99). A máscara é removida automaticamente antes do
    /// armazenamento. Deve ser único entre empresas ativas.
    /// </summary>
    /// <example>AB.CDE.FGH/0001-99</example>
    [Required]
    [MinLength(14)]
    [MaxLength(18)]
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
