using System.ComponentModel.DataAnnotations;

namespace EAuditoria.Application.DTOs.Request;

/// <summary>Dados para registrar a entrega de uma obrigação fiscal.</summary>
public class RegistrarEntregaRequest
{
    /// <summary>
    /// Data em que a obrigação foi efetivamente entregue/transmitida ao fisco.
    /// Não pode ser data futura.
    /// </summary>
    /// <example>2025-06-15T00:00:00Z</example>
    [Required]
    public DateTime DataEntrega { get; set; }

    /// <summary>
    /// Observação livre sobre a entrega (número de protocolo, sistema utilizado etc.).
    /// Campo opcional.
    /// </summary>
    /// <example>Protocolo SEFAZ 202506150001</example>
    [MaxLength(500)]
    public string? Observacao { get; set; }
}
