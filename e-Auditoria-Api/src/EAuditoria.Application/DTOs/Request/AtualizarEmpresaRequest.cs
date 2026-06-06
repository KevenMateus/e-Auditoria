using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Request;

/// <summary>Dados para atualização de uma empresa existente.</summary>
public class AtualizarEmpresaRequest
{
    /// <summary>Nova razão social da empresa.</summary>
    /// <example>Contabilidade ABC &amp; Associados Ltda</example>
    [Required]
    [MaxLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>
    /// Novo regime tributário. Ao alterar o regime, as obrigações geradas
    /// a partir do próximo ciclo refletirão as novas regras fiscais.
    /// </summary>
    /// <example>LucroPresumido</example>
    [Required]
    [DefaultValue(RegimeTributario.SimplesNacional)]
    public RegimeTributario RegimeTributario { get; set; }
}
