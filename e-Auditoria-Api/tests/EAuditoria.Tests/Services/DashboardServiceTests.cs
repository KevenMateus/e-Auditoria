using EAuditoria.Application.Services;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EAuditoria.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IObrigacaoRepository> _obrigacaoRepo = new();
    private readonly DashboardService _service;

    private static readonly Empresa _empresa =
        new("Empresa Teste", "11222333000181", RegimeTributario.LucroReal);

    public DashboardServiceTests()
    {
        _service = new DashboardService(_obrigacaoRepo.Object);
    }

    // ----------------------------------------------------------------
    // ObterAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task Obter_DeveRetornarContadoresCorretos()
    {
        var counts = new DashboardCounts(
            TotalEmpresas: 10,
            ObrigacoesMes: 50,
            Pendentes: 20,
            Entregues: 25,
            Atrasadas: 5);

        _obrigacaoRepo.Setup(r => r.ObterContagensDashboardAsync(6, 2025)).ReturnsAsync(counts);

        var result = await _service.ObterAsync(6, 2025);

        result.TotalEmpresas.Should().Be(10);
        result.ObrigacoesMes.Should().Be(50);
        result.Pendentes.Should().Be(20);
        result.Entregues.Should().Be(25);
        result.Atrasadas.Should().Be(5);
        result.Mes.Should().Be(6);
        result.Ano.Should().Be(2025);
    }

    [Fact]
    public async Task Obter_QuandoBancoVazio_DeveRetornarZeros()
    {
        _obrigacaoRepo.Setup(r => r.ObterContagensDashboardAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new DashboardCounts(0, 0, 0, 0, 0));

        var result = await _service.ObterAsync(1, 2025);

        result.TotalEmpresas.Should().Be(0);
        result.ObrigacoesMes.Should().Be(0);
    }

    [Fact]
    public async Task Obter_DevePropagarMesEAno()
    {
        _obrigacaoRepo.Setup(r => r.ObterContagensDashboardAsync(12, 2024))
            .ReturnsAsync(new DashboardCounts(5, 30, 10, 15, 5));

        var result = await _service.ObterAsync(12, 2024);

        result.Mes.Should().Be(12);
        result.Ano.Should().Be(2024);
    }

    // ----------------------------------------------------------------
    // ObterAlertasAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ObterAlertas_DeveUnirVencendoEAtrasadas()
    {
        var vencendo = new List<ObrigacaoAcessoria>
        {
            CriarObrigacao(DateTime.UtcNow.AddDays(5)),
            CriarObrigacao(DateTime.UtcNow.AddDays(15)),
        };
        var atrasadas = new List<ObrigacaoAcessoria>
        {
            CriarObrigacao(DateTime.UtcNow.AddDays(-3)),
        };

        _obrigacaoRepo.Setup(r => r.ObterVencendoEmDiasAsync(30)).ReturnsAsync(vencendo);
        _obrigacaoRepo.Setup(r => r.ObterAtrasadasAsync()).ReturnsAsync(atrasadas);

        var result = (await _service.ObterAlertasAsync()).ToList();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task ObterAlertas_DeveOrdenarPorUrgencia_AtrasadasPrimeiro()
    {
        var vencendo = new List<ObrigacaoAcessoria>
        {
            CriarObrigacao(DateTime.UtcNow.AddDays(20)),
            CriarObrigacao(DateTime.UtcNow.AddDays(5)),
        };
        var atrasadas = new List<ObrigacaoAcessoria>
        {
            CriarObrigacao(DateTime.UtcNow.AddDays(-10)),
        };

        _obrigacaoRepo.Setup(r => r.ObterVencendoEmDiasAsync(30)).ReturnsAsync(vencendo);
        _obrigacaoRepo.Setup(r => r.ObterAtrasadasAsync()).ReturnsAsync(atrasadas);

        var result = (await _service.ObterAlertasAsync()).ToList();

        // Atrasada (diasRestantes negativo) deve vir primeiro
        result.First().DiasRestantes.Should().BeNegative();
        // A que vence mais cedo vem antes da que vence mais tarde
        result[1].DiasRestantes.Should().BeLessThan(result[2].DiasRestantes);
    }

    [Fact]
    public async Task ObterAlertas_DeveEliminarDuplicatas()
    {
        // Mesma instância aparece nas duas listas
        var obrigacao = CriarObrigacao(DateTime.UtcNow.AddDays(-1));

        _obrigacaoRepo.Setup(r => r.ObterVencendoEmDiasAsync(30)).ReturnsAsync([obrigacao]);
        _obrigacaoRepo.Setup(r => r.ObterAtrasadasAsync()).ReturnsAsync([obrigacao]);

        var result = (await _service.ObterAlertasAsync()).ToList();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterAlertas_QuandoNenhum_DeveRetornarListaVazia()
    {
        _obrigacaoRepo.Setup(r => r.ObterVencendoEmDiasAsync(30)).ReturnsAsync([]);
        _obrigacaoRepo.Setup(r => r.ObterAtrasadasAsync()).ReturnsAsync([]);

        var result = await _service.ObterAlertasAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterAlertas_DeveTerDescricoesPreenchidas()
    {
        var obrigacao = CriarObrigacao(DateTime.UtcNow.AddDays(3));

        _obrigacaoRepo.Setup(r => r.ObterVencendoEmDiasAsync(30)).ReturnsAsync([obrigacao]);
        _obrigacaoRepo.Setup(r => r.ObterAtrasadasAsync()).ReturnsAsync([]);

        var result = (await _service.ObterAlertasAsync()).ToList();

        result.Single().TipoDescricao.Should().NotBeNullOrEmpty();
        result.Single().StatusDescricao.Should().NotBeNullOrEmpty();
    }

    // ----------------------------------------------------------------
    // Helper — cria ObrigacaoAcessoria sem navigation property Empresa
    // DashboardService usa o.Empresa?.RazaoSocial com null-coalescing,
    // então null retorna string.Empty sem lançar exceção
    // ----------------------------------------------------------------

    private static ObrigacaoAcessoria CriarObrigacao(DateTime vencimento) =>
        new ObrigacaoAcessoria(
            _empresa.Id,
            TipoObrigacao.DCTF,
            PeriodicidadeObrigacao.Mensal,
            6, 2025,
            vencimento);
}
