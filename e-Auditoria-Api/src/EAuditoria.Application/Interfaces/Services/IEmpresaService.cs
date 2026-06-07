using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Domain.Entities;

namespace EAuditoria.Application.Interfaces.Services;

public interface IEmpresaService
{
    Task<IEnumerable<EmpresaResponse>> ListarAsync();
    Task<IEnumerable<EmpresaResponse>> ListarInativasAsync();
    Task<EmpresaResponse?> ObterPorIdAsync(Guid id);

    /// <summary>Retorna a entidade de domínio (usada internamente por outros serviços que precisam da entidade).</summary>
    Task<Empresa?> ObterEntidadePorIdAsync(Guid id);

    Task<EmpresaResponse> CriarAsync(CriarEmpresaRequest request);
    Task<EmpresaResponse> AtualizarAsync(Guid id, AtualizarEmpresaRequest request);
    Task RemoverAsync(Guid id);
    Task<EmpresaResponse> ReativarAsync(Guid id);
}
