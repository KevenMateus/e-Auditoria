using System.Linq.Expressions;
using EAuditoria.Domain.Interfaces;
using EAuditoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected BaseRepository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<T?> ObterPorIdAsync(Guid id) =>
        await DbSet.FindAsync(id);

    public async Task<IEnumerable<T>> ObterTodosAsync() =>
        await DbSet.ToListAsync();

    public async Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicado) =>
        await DbSet.Where(predicado).ToListAsync();

    public async Task AdicionarAsync(T entidade) =>
        await DbSet.AddAsync(entidade);

    public void Atualizar(T entidade) =>
        DbSet.Update(entidade);

    public void Remover(T entidade) =>
        DbSet.Remove(entidade);

    public async Task<int> SalvarAsync() =>
        await Context.SaveChangesAsync();
}
