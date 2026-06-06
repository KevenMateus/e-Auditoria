using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Domain.Interfaces;

public interface IObrigacaoRepository : IRepository<ObrigacaoAcessoria>
{
    Task<IEnumerable<ObrigacaoAcessoria>> ObterPorEmpresaEMesAsync(Guid empresaId, int mes, int ano);
    Task<IEnumerable<ObrigacaoAcessoria>> ObterVencendoEmDiasAsync(int dias);
    Task<IEnumerable<ObrigacaoAcessoria>> ObterAtrasadasAsync();
    Task<ObrigacaoAcessoria?> ObterComEntregaAsync(Guid id);
    Task<bool> ExisteObrigacaoAsync(Guid empresaId, TipoObrigacao tipo, int competencia, int ano);
    Task<DashboardCounts> ObterContagensDashboardAsync(int mes, int ano);
}

public record DashboardCounts(
    int TotalEmpresas,
    int ObrigacoesMes,
    int Pendentes,
    int Entregues,
    int Atrasadas
);
