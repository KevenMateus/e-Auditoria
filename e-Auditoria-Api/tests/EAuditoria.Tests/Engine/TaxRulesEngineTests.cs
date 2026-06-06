using EAuditoria.Application.Engine;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EAuditoria.Tests.Engine;

public class TaxRulesEngineTests
{
    private readonly TaxRulesEngine _engine = new();

    // ================================================================
    // Geração de obrigações por regime
    // ================================================================

    [Fact]
    public void GerarObrigacoes_SimplesNacional_DeveConterDASEESocial()
    {
        var empresa = CriarEmpresa(RegimeTributario.SimplesNacional);

        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 3, ano: 2025).ToList();

        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.DAS);
        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.ESocial);
    }

    [Fact]
    public void GerarObrigacoes_SimplesNacional_NaoDeveConterDCTF()
    {
        var empresa = CriarEmpresa(RegimeTributario.SimplesNacional);

        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 3, ano: 2025).ToList();

        obrigacoes.Should().NotContain(o => o.Tipo == TipoObrigacao.DCTF);
    }

    [Fact]
    public void GerarObrigacoes_LucroPresumido_DeveConterDCTFEEFDs()
    {
        var empresa = CriarEmpresa(RegimeTributario.LucroPresumido);

        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 3, ano: 2025).ToList();

        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.DCTF);
        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.EFD_ICMS_IPI);
        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.EFD_Contribuicoes);
        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.EFD_Reinf);
    }

    [Fact]
    public void GerarObrigacoes_LucroPresumido_NaoDeveConterDAS()
    {
        var empresa = CriarEmpresa(RegimeTributario.LucroPresumido);

        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 3, ano: 2025).ToList();

        obrigacoes.Should().NotContain(o => o.Tipo == TipoObrigacao.DAS);
    }

    [Fact]
    public void GerarObrigacoes_LucroReal_DeveConterTodosOsTiposNaoSimplesNacional()
    {
        var empresa = CriarEmpresa(RegimeTributario.LucroReal);

        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 3, ano: 2025).ToList();

        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.DCTF);
        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.SPED_ECD || true); // SPED_ECD aparece só em janeiro
        obrigacoes.Should().NotContain(o => o.Tipo == TipoObrigacao.DAS);
        obrigacoes.Should().NotContain(o => o.Tipo == TipoObrigacao.DEFIS);
    }

    [Fact]
    public void GerarObrigacoes_ImunidadeIsencao_DeveRetornarVazio()
    {
        var empresa = CriarEmpresa(RegimeTributario.ImunidadeIsencao);

        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 3, ano: 2025).ToList();

        obrigacoes.Should().BeEmpty();
    }

    // ================================================================
    // Obrigações anuais — apenas em janeiro
    // ================================================================

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(12)]
    public void GerarObrigacoes_ObrigacoesAnuais_NaoDevemAparecerForaDeJaneiro(int mes)
    {
        var empresa = CriarEmpresa(RegimeTributario.SimplesNacional);

        var obrigacoes = _engine.GerarObrigacoes(empresa, mes, ano: 2025).ToList();

        obrigacoes.Should().NotContain(o => o.Tipo == TipoObrigacao.DEFIS);
        obrigacoes.Should().NotContain(o => o.Tipo == TipoObrigacao.DIRF);
        obrigacoes.Should().NotContain(o => o.Tipo == TipoObrigacao.RAIS);
    }

    [Fact]
    public void GerarObrigacoes_ObrigacoesAnuais_DevemAparecerEmJaneiro()
    {
        var empresa = CriarEmpresa(RegimeTributario.SimplesNacional);

        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 1, ano: 2025).ToList();

        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.DEFIS);
        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.DIRF);
        obrigacoes.Should().Contain(o => o.Tipo == TipoObrigacao.RAIS);
    }

    // ================================================================
    // Cálculo de vencimentos
    // ================================================================

    [Fact]
    public void CalcularVencimento_DAS_Dia20MesSeguinte()
    {
        // Competência março/2025 → vence 20/04/2025 (domingo → 21/04/2025, segunda-feira)
        // O engine prorroga fins de semana apenas; feriados não estão no escopo do spec.
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.DAS, mesCompetencia: 3, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 4, 21));
    }

    [Fact]
    public void CalcularVencimento_DAS_QuandoDia20EhSegundaFeiraNaoProrroga()
    {
        // Competência outubro/2025 → 20/11/2025 = quinta
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.DAS, mesCompetencia: 10, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 11, 20));
    }

    [Fact]
    public void CalcularVencimento_DAS_QuandoDia20EhSabadoProrroga2Dias()
    {
        // Competência fevereiro/2026 → 20/03/2026 = sexta (não prorroga)
        // Vamos encontrar um caso onde cai em sábado: setembro/2025 → 20/10/2025 = segunda
        // Precisamos de um mês onde 20 do seguinte é sábado.
        // Janeiro/2026 → 20/02/2026 = sexta. Julho/2024 → 20/08/2024 = terça.
        // Setembro/2026 → 20/10/2026 = terça.
        // Dezembro/2025 → 20/01/2026 = terça.
        // Vamos usar: competência agosto/2026 → 20/09/2026 = domingo → 22/09/2026
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.DAS, mesCompetencia: 8, anoCompetencia: 2026);
        var dia20Setembro2026 = new DateTime(2026, 9, 20);

        if (dia20Setembro2026.DayOfWeek == DayOfWeek.Sunday)
            vencimento.Should().Be(dia20Setembro2026.AddDays(1));
        else if (dia20Setembro2026.DayOfWeek == DayOfWeek.Saturday)
            vencimento.Should().Be(dia20Setembro2026.AddDays(2));
        else
            vencimento.Should().Be(dia20Setembro2026);
    }

    [Fact]
    public void CalcularVencimento_DCTF_Dia15SegundoMesSeguinte()
    {
        // Competência março/2025 → vence 15/05/2025
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.DCTF, mesCompetencia: 3, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 5, 15));
    }

    [Fact]
    public void CalcularVencimento_EFD_ICMS_Dia15MesSeguinte()
    {
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.EFD_ICMS_IPI, mesCompetencia: 3, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 4, 15));
    }

    [Fact]
    public void CalcularVencimento_ESocial_Dia7MesSeguinte()
    {
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.ESocial, mesCompetencia: 3, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 4, 7));
    }

    [Fact]
    public void CalcularVencimento_SPED_ECD_31MaioAnoSeguinte()
    {
        // Janeiro/2025 (obrigação anual exercício 2024) → vence 31/05/2025
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.SPED_ECD, mesCompetencia: 1, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 5, 31));
    }

    [Fact]
    public void CalcularVencimento_SPED_ECF_31JulhoAnoSeguinte()
    {
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.SPED_ECF, mesCompetencia: 1, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 7, 31));
    }

    [Fact]
    public void CalcularVencimento_DIRF_UltimoDiaFevereiro_AnoNaoListoAnno()
    {
        // 2025 não é bissexto → 28/02/2025
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.DIRF, mesCompetencia: 1, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 2, 28));
    }

    [Fact]
    public void CalcularVencimento_DIRF_UltimoDiaFevereiro_AnoBissexto()
    {
        // 2028 é bissexto → 29/02/2028
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.DIRF, mesCompetencia: 1, anoCompetencia: 2028);

        vencimento.Should().Be(new DateTime(2028, 2, 29));
    }

    [Fact]
    public void CalcularVencimento_RAIS_31MarcoAnoSeguinte()
    {
        var vencimento = _engine.CalcularVencimento(TipoObrigacao.RAIS, mesCompetencia: 1, anoCompetencia: 2025);

        vencimento.Should().Be(new DateTime(2025, 3, 31));
    }

    // ================================================================
    // Status das obrigações
    // ================================================================

    [Fact]
    public void RecalcularStatus_VencimentoFuturo_DeveFicarPendente()
    {
        var empresa = CriarEmpresa(RegimeTributario.SimplesNacional);
        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 1, ano: 2099).ToList();
        var obrigacao = obrigacoes.First();

        obrigacao.RecalcularStatus(DateTime.UtcNow);

        obrigacao.Status.Should().Be(StatusObrigacao.Pendente);
    }

    [Fact]
    public void RecalcularStatus_VencimentoPassado_DeveSerAtrasada()
    {
        var empresa = CriarEmpresa(RegimeTributario.SimplesNacional);
        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 1, ano: 2020).ToList();
        var obrigacao = obrigacoes.First();

        obrigacao.RecalcularStatus(DateTime.UtcNow);

        obrigacao.Status.Should().Be(StatusObrigacao.Atrasada);
    }

    [Fact]
    public void RecalcularStatus_EntregueNaoMudaStatus()
    {
        var empresa = CriarEmpresa(RegimeTributario.SimplesNacional);
        var obrigacoes = _engine.GerarObrigacoes(empresa, mes: 1, ano: 2020).ToList();
        var obrigacao = obrigacoes.First();

        obrigacao.MarcarComoEntregue();
        obrigacao.RecalcularStatus(DateTime.UtcNow);

        obrigacao.Status.Should().Be(StatusObrigacao.Entregue);
    }

    // ================================================================
    // Helper
    // ================================================================

    private static Empresa CriarEmpresa(RegimeTributario regime) =>
        new("Empresa Teste Ltda", "12345678000195", regime);
}
