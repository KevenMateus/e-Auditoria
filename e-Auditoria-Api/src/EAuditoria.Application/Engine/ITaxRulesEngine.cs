using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.Engine;

public interface ITaxRulesEngine
{
    IEnumerable<ObrigacaoAcessoria> GerarObrigacoes(Empresa empresa, int mes, int ano);
    DateTime CalcularVencimento(TipoObrigacao tipo, int mesCompetencia, int anoCompetencia);
}
