using System.ComponentModel.DataAnnotations;

namespace EAuditoria.Application.DTOs.Request;

/// <summary>Parâmetros para geração de obrigações de uma empresa em uma competência.</summary>
public class GerarObrigacoesRequest
{
    /// <summary>ID da empresa para a qual as obrigações serão geradas.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public Guid EmpresaId { get; set; }

    /// <summary>Mês de competência (1–12).</summary>
    /// <example>6</example>
    [Required]
    [Range(1, 12)]
    public int Mes { get; set; }

    /// <summary>Ano de competência (ex.: 2025).</summary>
    /// <example>2025</example>
    [Required]
    [Range(2000, 2100)]
    public int Ano { get; set; }
}
