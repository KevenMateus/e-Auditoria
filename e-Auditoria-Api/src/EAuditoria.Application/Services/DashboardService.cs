using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Helpers;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Interfaces;

namespace EAuditoria.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IObrigacaoRepository _obrigacaoRepository;

    public DashboardService(IObrigacaoRepository obrigacaoRepository)
    {
        _obrigacaoRepository = obrigacaoRepository;
    }

    public async Task<DashboardResponse> ObterAsync(int mes, int ano)
    {
        var counts = await _obrigacaoRepository.ObterContagensDashboardAsync(mes, ano);

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
        var hoje = DateTime.UtcNow;

        var vencendo  = await _obrigacaoRepository.ObterVencendoEmDiasAsync(30);
        var atrasadas = await _obrigacaoRepository.ObterAtrasadasAsync();

        var alertas = vencendo
            .UnionBy(atrasadas, o => o.Id)
            .Select(o => new AlertaObrigacaoResponse
            {
                ObrigacaoId      = o.Id,
                EmpresaId        = o.EmpresaId,
                EmpresaNome      = o.Empresa?.RazaoSocial ?? string.Empty,
                Cnpj             = o.Empresa?.Cnpj ?? string.Empty,
                Tipo             = o.Tipo,
                TipoDescricao    = o.Tipo.Descricao(),
                Vencimento       = o.Vencimento,
                DiasRestantes    = (int)(o.Vencimento.Date - hoje.Date).TotalDays,
                Status           = o.Status,
                StatusDescricao  = o.Status.Descricao()
            })
            .OrderBy(a => a.DiasRestantes)
            .ToList();

        return alertas;
    }
}
