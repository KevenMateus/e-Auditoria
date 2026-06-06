using EAuditoria.Application.DTOs.Response;

namespace EAuditoria.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardResponse> ObterAsync(int mes, int ano);
    Task<IEnumerable<AlertaObrigacaoResponse>> ObterAlertasAsync();
}
