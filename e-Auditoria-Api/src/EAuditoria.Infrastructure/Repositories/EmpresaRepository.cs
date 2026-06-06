using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Interfaces;
using EAuditoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.Infrastructure.Repositories;

public class EmpresaRepository : BaseRepository<Empresa>, IEmpresaRepository
{
    public EmpresaRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ExisteCnpjAsync(string cnpj, Guid? excluirId = null)
    {
        var query = DbSet.Where(e => e.Cnpj == cnpj && e.Ativo);

        if (excluirId.HasValue)
            query = query.Where(e => e.Id != excluirId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Empresa>> ObterAtivasAsync() =>
        await DbSet
            .Where(e => e.Ativo)
            .OrderBy(e => e.RazaoSocial)
            .AsNoTracking()
            .ToListAsync();

}
