using EAuditoria.Domain.Entities;

namespace EAuditoria.Application.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorIdAsync(Guid id);
    Task AdicionarAsync(Usuario usuario);
    Task<bool> ExisteEmailAsync(string email);
    Task<int> SalvarAsync();
}
