using EAuditoria.Application.Interfaces.Repositories;
using EAuditoria.Domain.Entities;
using EAuditoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EAuditoria.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<Usuario?> ObterPorIdAsync(Guid id) =>
        await _context.Usuarios.FindAsync(id);

    public async Task AdicionarAsync(Usuario usuario) =>
        await _context.Usuarios.AddAsync(usuario);

    public async Task<bool> ExisteEmailAsync(string email) =>
        await _context.Usuarios.AnyAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<int> SalvarAsync() =>
        await _context.SaveChangesAsync();
}
