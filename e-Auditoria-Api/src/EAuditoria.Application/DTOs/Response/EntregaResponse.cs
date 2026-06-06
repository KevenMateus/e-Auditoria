namespace EAuditoria.Application.DTOs.Response;

public class EntregaResponse
{
    public Guid Id { get; set; }
    public Guid ObrigacaoId { get; set; }
    public DateTime DataEntrega { get; set; }
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; }
}
