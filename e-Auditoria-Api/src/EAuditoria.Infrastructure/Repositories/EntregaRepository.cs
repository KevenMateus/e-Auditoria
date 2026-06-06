using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Interfaces;
using EAuditoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.Infrastructure.Repositories;

public class EntregaRepository : BaseRepository<EntregaObrigacao>, IEntregaRepository
{
    public EntregaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<EntregaObrigacao>> ObterHistoricoPorEmpresaAsync(Guid empresaId) =>
        await DbSet
            .Include(e => e.Obrigacao)
                .ThenInclude(o => o.Empresa)
            .Where(e => e.Obrigacao.EmpresaId == empresaId)
            .OrderByDescending(e => e.DataEntrega)
            .AsNoTracking()
            .ToListAsync();

}
