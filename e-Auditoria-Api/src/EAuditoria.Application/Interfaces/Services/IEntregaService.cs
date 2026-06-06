using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;

namespace EAuditoria.Application.Interfaces.Services;

public interface IEntregaService
{
    Task<EntregaResponse> RegistrarAsync(Guid obrigacaoId, RegistrarEntregaRequest request);
    Task<IEnumerable<EntregaResponse>> ObterHistoricoAsync(Guid empresaId);
}
