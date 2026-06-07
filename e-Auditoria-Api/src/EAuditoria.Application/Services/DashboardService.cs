using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;

namespace EAuditoria.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IObrigacaoService _obrigacaoService;

    public DashboardService(IObrigacaoService obrigacaoService)
    {
        _obrigacaoService = obrigacaoService;
    }

    public async Task<DashboardResponse> ObterAsync(int mes, int ano)
    {
        var counts = await _obrigacaoService.ObterContagensDashboardAsync(mes, ano);

        return new DashboardResponse
        {
            TotalEmpresas = counts.TotalEmpresas,
            ObrigacoesMes = counts.ObrigacoesMes,
            Pendentes     = counts.Pendentes,
            Entregues     = counts.Entregues,
            Atrasadas     = counts.Atrasadas,
            Mes           = mes,
            Ano           = ano
        };
    }

    public async Task<IEnumerable<AlertaObrigacaoResponse>> ObterAlertasAsync()
    {
        var vencendo  = await _obrigacaoService.ObterVencendoEmDiasAsync(30);
        var atrasadas = await _obrigacaoService.ObterAtrasadasAsync();

        return vencendo
            .UnionBy(atrasadas, o => o.ObrigacaoId)
            .OrderBy(a => a.DiasRestantes)
            .ToList();
    }
}
