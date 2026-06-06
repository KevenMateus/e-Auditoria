using EAuditoria.Domain.Enums;

namespace EAuditoria.Domain.Entities;

public class Empresa
{
    public Guid Id { get; private set; }
    public string RazaoSocial { get; private set; } = string.Empty;
    public string Cnpj { get; private set; } = string.Empty;
    public RegimeTributario RegimeTributario { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    public ICollection<ObrigacaoAcessoria> Obrigacoes { get; private set; } = new List<ObrigacaoAcessoria>();

    protected Empresa() { }

    public Empresa(string razaoSocial, string cnpj, RegimeTributario regimeTributario)
    {
        Id = Guid.NewGuid();
        RazaoSocial = razaoSocial;
        Cnpj = cnpj;
        RegimeTributario = regimeTributario;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string razaoSocial, RegimeTributario regimeTributario)
    {
        RazaoSocial = razaoSocial;
        RegimeTributario = regimeTributario;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}
