using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;

namespace EAuditoria.Application.Interfaces.Services;

public interface IEmpresaService
{
    Task<IEnumerable<EmpresaResponse>> ListarAsync();
    Task<EmpresaResponse?> ObterPorIdAsync(Guid id);
    Task<EmpresaResponse> CriarAsync(CriarEmpresaRequest request);
    Task<EmpresaResponse> AtualizarAsync(Guid id, AtualizarEmpresaRequest request);
    Task RemoverAsync(Guid id);
}
