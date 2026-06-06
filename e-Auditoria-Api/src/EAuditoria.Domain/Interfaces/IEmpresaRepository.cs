using EAuditoria.Domain.Entities;

namespace EAuditoria.Domain.Interfaces;

public interface IEmpresaRepository : IRepository<Empresa>
{
    Task<bool> ExisteCnpjAsync(string cnpj, Guid? excluirId = null);
    Task<IEnumerable<Empresa>> ObterAtivasAsync();
    Task<Empresa?> ObterComObrigacoesAsync(Guid id);
}
