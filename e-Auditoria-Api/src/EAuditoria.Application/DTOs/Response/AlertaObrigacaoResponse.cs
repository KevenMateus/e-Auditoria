using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Response;

public class AlertaObrigacaoResponse
{
    public Guid ObrigacaoId { get; set; }
    public Guid EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public TipoObrigacao Tipo { get; set; }
    public string TipoDescricao { get; set; } = string.Empty;
    public DateTime Vencimento { get; set; }
    public int DiasRestantes { get; set; }
    public StatusObrigacao Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
}
