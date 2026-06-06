using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.Interfaces.Services;

public interface IObrigacaoService
{
    Task<IEnumerable<ObrigacaoResponse>> ObterCalendarioAsync(Guid empresaId, int mes, int ano, StatusObrigacao? filtroStatus = null);
    Task<IEnumerable<ObrigacaoResponse>> GerarObrigacoesAsync(GerarObrigacoesRequest request);
    Task<ObrigacaoResponse?> ObterPorIdAsync(Guid id);
    Task<byte[]> ExportarCsvAsync(Guid empresaId, int mes, int ano);
}
