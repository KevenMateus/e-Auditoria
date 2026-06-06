namespace EAuditoria.Application.DTOs.Response;

/// <summary>Registro de entrega de uma obrigação fiscal.</summary>
public class EntregaResponse
{
    /// <summary>Identificador único da entrega.</summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid Id { get; set; }

    /// <summary>ID da obrigação que foi entregue.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid ObrigacaoId { get; set; }

    /// <summary>Data em que a obrigação foi efetivamente entregue ao fisco.</summary>
    /// <example>2025-06-15T00:00:00Z</example>
    public DateTime DataEntrega { get; set; }

    /// <summary>Observação opcional registrada no momento da entrega.</summary>
    /// <example>Protocolo SEFAZ 202506150001</example>
    public string? Observacao { get; set; }

    /// <summary>Data e hora em que o registro foi criado no sistema (UTC).</summary>
    /// <example>2025-06-15T10:22:00Z</example>
    public DateTime CriadoEm { get; set; }
}
