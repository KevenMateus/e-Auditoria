using EAuditoria.Domain.Entities;

namespace EAuditoria.Domain.Interfaces;

public interface IEntregaRepository : IRepository<EntregaObrigacao>
{
    Task<IEnumerable<EntregaObrigacao>> ObterHistoricoPorEmpresaAsync(Guid empresaId);
    Task<EntregaObrigacao?> ObterPorObrigacaoAsync(Guid obrigacaoId);
}
