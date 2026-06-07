using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
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
    private readonly Mock<IObrigacaoService> _obrigacaoService = new();
    private readonly DashboardService _service;

    private static readonly Empresa _empresa =
        new("Empresa Teste", "11222333000181", RegimeTributario.LucroReal);

    public DashboardServiceTests()
    {
        _service = new DashboardService(_obrigacaoService.Object);
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

        _obrigacaoService.Setup(s => s.ObterContagensDashboardAsync(6, 2025)).ReturnsAsync(counts);

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
        _obrigacaoService.Setup(s => s.ObterContagensDashboardAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new DashboardCounts(0, 0, 0, 0, 0));

        var result = await _service.ObterAsync(1, 2025);

        result.TotalEmpresas.Should().Be(0);
        result.ObrigacoesMes.Should().Be(0);
    }

    [Fact]
    public async Task Obter_DevePropagarMesEAno()
    {
        _obrigacaoService.Setup(s => s.ObterContagensDashboardAsync(12, 2024))
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
        var vencendo = new List<AlertaObrigacaoResponse>
        {
            CriarAlerta(_empresa.Id, DateTime.UtcNow.AddDays(5)),
            CriarAlerta(_empresa.Id, DateTime.UtcNow.AddDays(15)),
        };
        var atrasadas = new List<AlertaObrigacaoResponse>
        {
            CriarAlerta(_empresa.Id, DateTime.UtcNow.AddDays(-3)),
        };

        _obrigacaoService.Setup(s => s.ObterVencendoEmDiasAsync(30)).ReturnsAsync(vencendo);
        _obrigacaoService.Setup(s => s.ObterAtrasadasAsync()).ReturnsAsync(atrasadas);

        var result = (await _service.ObterAlertasAsync()).ToList();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task ObterAlertas_DeveOrdenarPorUrgencia_AtrasadasPrimeiro()
    {
        var vencendo = new List<AlertaObrigacaoResponse>
        {
            CriarAlerta(_empresa.Id, DateTime.UtcNow.AddDays(20)),
            CriarAlerta(_empresa.Id, DateTime.UtcNow.AddDays(5)),
        };
        var atrasadas = new List<AlertaObrigacaoResponse>
        {
            CriarAlerta(_empresa.Id, DateTime.UtcNow.AddDays(-10)),
        };

        _obrigacaoService.Setup(s => s.ObterVencendoEmDiasAsync(30)).ReturnsAsync(vencendo);
        _obrigacaoService.Setup(s => s.ObterAtrasadasAsync()).ReturnsAsync(atrasadas);

        var result = (await _service.ObterAlertasAsync()).ToList();

        // Atrasada (diasRestantes negativo) deve vir primeiro
        result.First().DiasRestantes.Should().BeNegative();
        // A que vence mais cedo vem antes da que vence mais tarde
        result[1].DiasRestantes.Should().BeLessThan(result[2].DiasRestantes);
    }

    [Fact]
    public async Task ObterAlertas_DeveEliminarDuplicatas()
    {
        // Mesmo ID aparece nas duas listas
        var alerta = CriarAlerta(_empresa.Id, DateTime.UtcNow.AddDays(-1));

        _obrigacaoService.Setup(s => s.ObterVencendoEmDiasAsync(30)).ReturnsAsync([alerta]);
        _obrigacaoService.Setup(s => s.ObterAtrasadasAsync()).ReturnsAsync([alerta]);

        var result = (await _service.ObterAlertasAsync()).ToList();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterAlertas_QuandoNenhum_DeveRetornarListaVazia()
    {
        _obrigacaoService.Setup(s => s.ObterVencendoEmDiasAsync(30)).ReturnsAsync([]);
        _obrigacaoService.Setup(s => s.ObterAtrasadasAsync()).ReturnsAsync([]);

        var result = await _service.ObterAlertasAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterAlertas_DeveTerDescricoesPreenchidas()
    {
        var alerta = CriarAlerta(_empresa.Id, DateTime.UtcNow.AddDays(3));
        alerta.TipoDescricao = "DCTF — Declaração de Débitos";
        alerta.StatusDescricao = "Pendente";

        _obrigacaoService.Setup(s => s.ObterVencendoEmDiasAsync(30)).ReturnsAsync([alerta]);
        _obrigacaoService.Setup(s => s.ObterAtrasadasAsync()).ReturnsAsync([]);

        var result = (await _service.ObterAlertasAsync()).ToList();

        result.Single().TipoDescricao.Should().NotBeNullOrEmpty();
        result.Single().StatusDescricao.Should().NotBeNullOrEmpty();
    }

    // ----------------------------------------------------------------
    // Helper
    // ----------------------------------------------------------------

    private static AlertaObrigacaoResponse CriarAlerta(Guid empresaId, DateTime vencimento)
    {
        var hoje = DateTime.UtcNow.Date;
        var dias = (int)(vencimento.Date - hoje).TotalDays;
        return new AlertaObrigacaoResponse
        {
            ObrigacaoId    = Guid.NewGuid(),
            EmpresaId      = empresaId,
            EmpresaNome    = "Empresa Teste",
            Cnpj           = "11222333000181",
            Tipo           = TipoObrigacao.DCTF,
            TipoDescricao  = "DCTF",
            Vencimento     = vencimento,
            DiasRestantes  = dias,
            Status         = dias < 0 ? StatusObrigacao.Atrasada : StatusObrigacao.Pendente,
            StatusDescricao = dias < 0 ? "Atrasada" : "Pendente",
        };
    }
}
