using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Response;

public class ObrigacaoResponse
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public TipoObrigacao Tipo { get; set; }
    public string TipoDescricao { get; set; } = string.Empty;
    public PeriodicidadeObrigacao Periodicidade { get; set; }
    public int Competencia { get; set; }
    public int AnoCompetencia { get; set; }
    public DateTime Vencimento { get; set; }
    public StatusObrigacao Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public EntregaResponse? Entrega { get; set; }
}
