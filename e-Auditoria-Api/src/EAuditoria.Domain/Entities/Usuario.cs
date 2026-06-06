namespace EAuditoria.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string Perfil { get; private set; } = "Operador";
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? UltimoLoginEm { get; private set; }

    protected Usuario() { }

    public Usuario(string nome, string email, string senhaHash, string perfil = "Operador")
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email.ToLowerInvariant();
        SenhaHash = senhaHash;
        Perfil = perfil;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void RegistrarLogin()
    {
        UltimoLoginEm = DateTime.UtcNow;
    }

}
