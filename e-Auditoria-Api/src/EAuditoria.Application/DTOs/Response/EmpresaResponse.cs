using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Response;

/// <summary>Dados de uma empresa cadastrada no sistema.</summary>
public class EmpresaResponse
{
    /// <summary>Identificador único da empresa.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>Razão social da empresa.</summary>
    /// <example>Contabilidade ABC Ltda</example>
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>CNPJ da empresa (somente dígitos).</summary>
    /// <example>12345678000199</example>
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>Regime tributário (valor enum).</summary>
    /// <example>SimplesNacional</example>
    public RegimeTributario RegimeTributario { get; set; }

    /// <summary>Descrição legível do regime tributário.</summary>
    /// <example>Simples Nacional</example>
    public string RegimeTributarioDescricao { get; set; } = string.Empty;

    /// <summary>Indica se a empresa está ativa no sistema.</summary>
    /// <example>true</example>
    public bool Ativo { get; set; }

    /// <summary>Data e hora de cadastro (UTC).</summary>
    /// <example>2025-01-10T14:30:00Z</example>
    public DateTime CriadoEm { get; set; }
}
