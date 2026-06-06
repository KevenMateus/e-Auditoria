namespace EAuditoria.Domain.Entities;

public class EntregaObrigacao
{
    public Guid Id { get; private set; }
    public Guid ObrigacaoId { get; private set; }
    public DateTime DataEntrega { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public ObrigacaoAcessoria Obrigacao { get; private set; } = null!;

    protected EntregaObrigacao() { }

    public EntregaObrigacao(Guid obrigacaoId, DateTime dataEntrega, string? observacao = null)
    {
        Id = Guid.NewGuid();
        ObrigacaoId = obrigacaoId;
        DataEntrega = DateTime.SpecifyKind(dataEntrega, DateTimeKind.Utc);
        Observacao = observacao;
        CriadoEm = DateTime.UtcNow;
    }
}
