using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Response;

/// <summary>Obrigação fiscal de uma empresa para uma competência específica.</summary>
public class ObrigacaoResponse
{
    /// <summary>Identificador único da obrigação.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>ID da empresa a qual a obrigação pertence.</summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid EmpresaId { get; set; }

    /// <summary>Razão social da empresa.</summary>
    /// <example>Contabilidade ABC Ltda</example>
    public string EmpresaNome { get; set; } = string.Empty;

    /// <summary>Tipo da obrigação (valor enum).</summary>
    /// <example>DAS</example>
    public TipoObrigacao Tipo { get; set; }

    /// <summary>Descrição legível do tipo de obrigação.</summary>
    /// <example>DAS — Documento de Arrecadação do Simples Nacional</example>
    public string TipoDescricao { get; set; } = string.Empty;

    /// <summary>Periodicidade da obrigação.</summary>
    /// <example>Mensal</example>
    public PeriodicidadeObrigacao Periodicidade { get; set; }

    /// <summary>Mês de competência (1–12).</summary>
    /// <example>5</example>
    public int Competencia { get; set; }

    /// <summary>Ano de competência.</summary>
    /// <example>2025</example>
    public int AnoCompetencia { get; set; }

    /// <summary>
    /// Data de vencimento calculada conforme as regras fiscais do calendário brasileiro.
    /// Fins de semana e feriados nacionais são prorrogados automaticamente.
    /// </summary>
    /// <example>2025-06-20T00:00:00Z</example>
    public DateTime Vencimento { get; set; }

    /// <summary>Status atual da obrigação.</summary>
    /// <example>Pendente</example>
    public StatusObrigacao Status { get; set; }

    /// <summary>Descrição legível do status.</summary>
    /// <example>Pendente</example>
    public string StatusDescricao { get; set; } = string.Empty;

    /// <summary>Dados da entrega, se a obrigação já foi registrada como entregue. Nulo caso contrário.</summary>
    public EntregaResponse? Entrega { get; set; }
}
