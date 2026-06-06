namespace EAuditoria.Application.DTOs.Response;

public class DashboardResponse
{
    public int TotalEmpresas { get; set; }
    public int ObrigacoesMes { get; set; }
    public int Pendentes { get; set; }
    public int Entregues { get; set; }
    public int Atrasadas { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
}
