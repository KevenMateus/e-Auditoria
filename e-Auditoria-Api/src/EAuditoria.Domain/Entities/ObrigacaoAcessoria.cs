using EAuditoria.Domain.Enums;

namespace EAuditoria.Domain.Entities;

public class ObrigacaoAcessoria
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public TipoObrigacao Tipo { get; private set; }
    public PeriodicidadeObrigacao Periodicidade { get; private set; }
    public int Competencia { get; private set; }
    public int AnoCompetencia { get; private set; }
    public DateTime Vencimento { get; private set; }
    public StatusObrigacao Status { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public EntregaObrigacao? Entrega { get; private set; }

    protected ObrigacaoAcessoria() { }

    public ObrigacaoAcessoria(
        Guid empresaId,
        TipoObrigacao tipo,
        PeriodicidadeObrigacao periodicidade,
        int competencia,
        int anoCompetencia,
        DateTime vencimento)
    {
        Id = Guid.NewGuid();
        EmpresaId = empresaId;
        Tipo = tipo;
        Periodicidade = periodicidade;
        Competencia = competencia;
        AnoCompetencia = anoCompetencia;
        Vencimento = vencimento;
        Status = StatusObrigacao.Pendente;
        CriadoEm = DateTime.UtcNow;
    }

    public void RecalcularStatus(DateTime dataReferencia)
    {
        if (Status == StatusObrigacao.Entregue) return;

        Status = Vencimento.Date < dataReferencia.Date
            ? StatusObrigacao.Atrasada
            : StatusObrigacao.Pendente;
    }

    public void MarcarComoEntregue()
    {
        Status = StatusObrigacao.Entregue;
    }
}
