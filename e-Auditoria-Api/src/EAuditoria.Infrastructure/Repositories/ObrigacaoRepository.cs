using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Interfaces;
using EAuditoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.Infrastructure.Repositories;

public class ObrigacaoRepository : BaseRepository<ObrigacaoAcessoria>, IObrigacaoRepository
{
    public ObrigacaoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ObrigacaoAcessoria>> ObterPorEmpresaEMesAsync(Guid empresaId, int mes, int ano) =>
        await DbSet
            .Include(o => o.Empresa)
            .Include(o => o.Entrega)
            .Where(o => o.EmpresaId == empresaId && o.Competencia == mes && o.AnoCompetencia == ano)
            .OrderBy(o => o.Vencimento)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<ObrigacaoAcessoria>> ObterVencendoEmDiasAsync(int dias)
    {
        var agora = DateTime.UtcNow;
        var hoje = new DateTime(agora.Year, agora.Month, agora.Day, 0, 0, 0, DateTimeKind.Utc);
        var limite = hoje.AddDays(dias + 1);

        return await DbSet
            .Include(o => o.Empresa)
            .Where(o =>
                o.Status != StatusObrigacao.Entregue &&
                o.Vencimento >= hoje &&
                o.Vencimento < limite)
            .OrderBy(o => o.Vencimento)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<ObrigacaoAcessoria>> ObterAtrasadasAsync()
    {
        var agora = DateTime.UtcNow;
        var hoje = new DateTime(agora.Year, agora.Month, agora.Day, 0, 0, 0, DateTimeKind.Utc);

        return await DbSet
            .Include(o => o.Empresa)
            .Where(o =>
                o.Status != StatusObrigacao.Entregue &&
                o.Vencimento < hoje)
            .OrderBy(o => o.Vencimento)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ObrigacaoAcessoria?> ObterComEntregaAsync(Guid id) =>
        await DbSet
            .Include(o => o.Empresa)
            .Include(o => o.Entrega)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<bool> ExisteObrigacaoAsync(Guid empresaId, TipoObrigacao tipo, int competencia, int ano) =>
        await DbSet.AnyAsync(o =>
            o.EmpresaId == empresaId &&
            o.Tipo == tipo &&
            o.Competencia == competencia &&
            o.AnoCompetencia == ano);

    public async Task<DashboardCounts> ObterContagensDashboardAsync(int mes, int ano)
    {
        var agora = DateTime.UtcNow;
        var hoje = new DateTime(agora.Year, agora.Month, agora.Day, 0, 0, 0, DateTimeKind.Utc);

        var totalEmpresas = await Context.Empresas.CountAsync(e => e.Ativo);

        var obrigacoesMes = await DbSet
            .Where(o => o.Competencia == mes && o.AnoCompetencia == ano)
            .ToListAsync();

        foreach (var o in obrigacoesMes)
            o.RecalcularStatus(hoje);

        return new DashboardCounts(
            TotalEmpresas: totalEmpresas,
            ObrigacoesMes: obrigacoesMes.Count,
            Pendentes: obrigacoesMes.Count(o => o.Status == StatusObrigacao.Pendente),
            Entregues: obrigacoesMes.Count(o => o.Status == StatusObrigacao.Entregue),
            Atrasadas: obrigacoesMes.Count(o => o.Status == StatusObrigacao.Atrasada)
        );
    }
}
