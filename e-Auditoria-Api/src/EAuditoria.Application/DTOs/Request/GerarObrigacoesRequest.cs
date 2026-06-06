namespace EAuditoria.Application.DTOs.Request;

public class GerarObrigacoesRequest
{
    public Guid EmpresaId { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
}
