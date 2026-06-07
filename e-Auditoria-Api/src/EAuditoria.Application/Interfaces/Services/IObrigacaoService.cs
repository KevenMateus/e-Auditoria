using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Interfaces;

namespace EAuditoria.Application.Interfaces.Services;

public interface IObrigacaoService
{
    Task<IEnumerable<ObrigacaoResponse>> ObterCalendarioAsync(Guid empresaId, int mes, int ano, StatusObrigacao? filtroStatus = null);
    Task<IEnumerable<ObrigacaoResponse>> GerarObrigacoesAsync(GerarObrigacoesRequest request);
    Task<ObrigacaoResponse?> ObterPorIdAsync(Guid id);
    Task<byte[]> ExportarCsvAsync(Guid empresaId, int mes, int ano);

    // ── Métodos internos usados por outros Services ───────────────────────────

    /// <summary>Retorna a entidade de domínio de uma obrigação (com entrega inclusa).</summary>
    Task<ObrigacaoAcessoria?> ObterEntidadeComEntregaAsync(Guid obrigacaoId);

    /// <summary>Persiste a atualização de uma entidade de obrigação (ex.: marcar entregue).</summary>
    void AtualizarEntidade(ObrigacaoAcessoria obrigacao);

    /// <summary>Retorna contagens para o Dashboard, com status recalculado.</summary>
    Task<DashboardCounts> ObterContagensDashboardAsync(int mes, int ano);

    /// <summary>Obrigações pendentes vencendo dentro de N dias.</summary>
    Task<IEnumerable<AlertaObrigacaoResponse>> ObterVencendoEmDiasAsync(int dias);

    /// <summary>Obrigações não entregues com vencimento passado.</summary>
    Task<IEnumerable<AlertaObrigacaoResponse>> ObterAtrasadasAsync();

    /// <summary>Gera e persiste obrigações para uma empresa (usado ao criar/reativar empresa).</summary>
    Task GerarParaEmpresaAsync(Domain.Entities.Empresa empresa, int mes, int ano);

    /// <summary>Salva pendências de obrigações adicionadas via GerarParaEmpresaAsync.</summary>
    Task SalvarAsync();
}
