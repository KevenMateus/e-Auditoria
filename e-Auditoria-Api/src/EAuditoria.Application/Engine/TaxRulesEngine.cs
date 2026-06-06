using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.Engine;

public sealed class TaxRulesEngine : ITaxRulesEngine
{
    private static readonly Dictionary<TipoObrigacao, HashSet<RegimeTributario>> _obrigacoesPorRegime = new()
    {
        [TipoObrigacao.DAS]               = [RegimeTributario.SimplesNacional],
        [TipoObrigacao.DEFIS]             = [RegimeTributario.SimplesNacional],
        [TipoObrigacao.DCTF]              = [RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
        [TipoObrigacao.EFD_ICMS_IPI]      = [RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
        [TipoObrigacao.EFD_Contribuicoes] = [RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
        [TipoObrigacao.EFD_Reinf]         = [RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
        [TipoObrigacao.SPED_ECD]          = [RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
        [TipoObrigacao.SPED_ECF]          = [RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
        [TipoObrigacao.ESocial]           = [RegimeTributario.SimplesNacional, RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
        [TipoObrigacao.DIRF]              = [RegimeTributario.SimplesNacional, RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
        [TipoObrigacao.RAIS]              = [RegimeTributario.SimplesNacional, RegimeTributario.LucroPresumido, RegimeTributario.LucroReal],
    };

    private static readonly HashSet<TipoObrigacao> _obrigacoesAnuais =
    [
        TipoObrigacao.DEFIS,
        TipoObrigacao.SPED_ECD,
        TipoObrigacao.SPED_ECF,
        TipoObrigacao.DIRF,
        TipoObrigacao.RAIS,
    ];

    public IEnumerable<ObrigacaoAcessoria> GerarObrigacoes(Empresa empresa, int mes, int ano)
    {
        ArgumentNullException.ThrowIfNull(empresa);

        if (empresa.RegimeTributario == RegimeTributario.ImunidadeIsencao)
            yield break;

        foreach (var (tipo, regimes) in _obrigacoesPorRegime)
        {
            if (!regimes.Contains(empresa.RegimeTributario))
                continue;

            bool isAnual = _obrigacoesAnuais.Contains(tipo);

            if (isAnual && mes != 1)
                continue;

            var periodicidade = isAnual
                ? PeriodicidadeObrigacao.Anual
                : PeriodicidadeObrigacao.Mensal;

            var vencimento = CalcularVencimento(tipo, mes, ano);

            yield return new ObrigacaoAcessoria(
                empresa.Id,
                tipo,
                periodicidade,
                mes,
                ano,
                vencimento);
        }
    }

    public DateTime CalcularVencimento(TipoObrigacao tipo, int mesCompetencia, int anoCompetencia)
    {
        return tipo switch
        {
            TipoObrigacao.DAS => ProximoDiaUtil(
                Utc(anoCompetencia, mesCompetencia, 1).AddMonths(1).AddDays(19)),
            TipoObrigacao.DCTF => Utc(anoCompetencia, mesCompetencia, 1)
                .AddMonths(2)
                .AddDays(14),
            TipoObrigacao.EFD_ICMS_IPI => Utc(anoCompetencia, mesCompetencia, 1)
                .AddMonths(1)
                .AddDays(14),
            TipoObrigacao.EFD_Contribuicoes => Utc(anoCompetencia, mesCompetencia, 1)
                .AddMonths(1)
                .AddDays(14),
            TipoObrigacao.EFD_Reinf => Utc(anoCompetencia, mesCompetencia, 1)
                .AddMonths(1)
                .AddDays(14),
            TipoObrigacao.ESocial => Utc(anoCompetencia, mesCompetencia, 1)
                .AddMonths(1)
                .AddDays(6),
            TipoObrigacao.SPED_ECD => Utc(anoCompetencia, 5, 31),
            TipoObrigacao.SPED_ECF => Utc(anoCompetencia, 7, 31),
            TipoObrigacao.DIRF => UltimoDiaFevereiro(anoCompetencia),
            TipoObrigacao.RAIS  => Utc(anoCompetencia, 3, 31),
            TipoObrigacao.DEFIS => Utc(anoCompetencia, 3, 31),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), $"Tipo de obrigação não mapeado: {tipo}")
        };
    }

    private static DateTime Utc(int ano, int mes, int dia) =>
        new(ano, mes, dia, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime ProximoDiaUtil(DateTime data)
    {
        return data.DayOfWeek switch
        {
            DayOfWeek.Saturday => data.AddDays(2),
            DayOfWeek.Sunday   => data.AddDays(1),
            _                  => data
        };
    }

    private static DateTime UltimoDiaFevereiro(int ano) =>
        DateTime.IsLeapYear(ano)
            ? Utc(ano, 2, 29)
            : Utc(ano, 2, 28);
}
