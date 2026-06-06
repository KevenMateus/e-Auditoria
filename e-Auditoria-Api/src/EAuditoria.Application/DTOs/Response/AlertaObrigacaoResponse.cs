using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Response;

/// <summary>Alerta de obrigação vencendo em breve ou já atrasada.</summary>
public class AlertaObrigacaoResponse
{
    /// <summary>ID da obrigação relacionada.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid ObrigacaoId { get; set; }

    /// <summary>ID da empresa responsável pela obrigação.</summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid EmpresaId { get; set; }

    /// <summary>Razão social da empresa.</summary>
    /// <example>Contabilidade ABC Ltda</example>
    public string EmpresaNome { get; set; } = string.Empty;

    /// <summary>CNPJ da empresa (somente dígitos).</summary>
    /// <example>12345678000199</example>
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>Tipo da obrigação (valor enum).</summary>
    /// <example>DCTF</example>
    public TipoObrigacao Tipo { get; set; }

    /// <summary>Descrição legível do tipo de obrigação.</summary>
    /// <example>DCTF — Declaração de Débitos e Créditos Tributários Federais</example>
    public string TipoDescricao { get; set; } = string.Empty;

    /// <summary>Data de vencimento da obrigação.</summary>
    /// <example>2025-06-20T00:00:00Z</example>
    public DateTime Vencimento { get; set; }

    /// <summary>
    /// Dias restantes até o vencimento. Valor negativo indica obrigação já atrasada.
    /// Ordenado por urgência (mais atrasadas primeiro).
    /// </summary>
    /// <example>-3</example>
    public int DiasRestantes { get; set; }

    /// <summary>Status atual: <c>Pendente</c> (vence em breve) ou <c>Atrasada</c> (vencida).</summary>
    /// <example>Atrasada</example>
    public StatusObrigacao Status { get; set; }

    /// <summary>Descrição legível do status.</summary>
    /// <example>Atrasada</example>
    public string StatusDescricao { get; set; } = string.Empty;
}
