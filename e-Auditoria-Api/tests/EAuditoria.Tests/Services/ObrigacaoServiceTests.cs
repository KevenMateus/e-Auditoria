using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.Engine;
using EAuditoria.Application.Services;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EAuditoria.Tests.Services;

public class ObrigacaoServiceTests
{
    private readonly Mock<IObrigacaoRepository> _obrigacaoRepo = new();
    private readonly Mock<IEmpresaRepository> _empresaRepo = new();
    private readonly Mock<ITaxRulesEngine> _engine = new();
    private readonly ObrigacaoService _service;

    private static readonly Empresa _empresa =
        new("Empresa Teste", "11222333000181", RegimeTributario.SimplesNacional);

    public ObrigacaoServiceTests()
    {
        _service = new ObrigacaoService(
            _obrigacaoRepo.Object,
            _empresaRepo.Object,
            _engine.Object,
            MapperFixture.Create());
    }

    // ----------------------------------------------------------------
    // ObterCalendario
    // ----------------------------------------------------------------

    [Fact]
    public async Task ObterCalendario_DeveRetornarObrigacoesComStatusRecalculado()
    {
        var vencimentoPassado = DateTime.UtcNow.AddDays(-5);
        var obrigacao = CriarObrigacao(StatusObrigacao.Pendente, vencimentoPassado);

        _obrigacaoRepo.Setup(r => r.ObterPorEmpresaEMesAsync(_empresa.Id, 6, 2025))
            .ReturnsAsync([obrigacao]);

        var result = await _service.ObterCalendarioAsync(_empresa.Id, 6, 2025);

        result.Single().Status.Should().Be(StatusObrigacao.Atrasada);
    }

    [Fact]
    public async Task ObterCalendario_ComFiltroStatus_DeveRetornarApenasDoStatus()
    {
        var obrigacaoPendente = CriarObrigacao(StatusObrigacao.Pendente, DateTime.UtcNow.AddDays(10));
        var obrigacaoAtrasada = CriarObrigacao(StatusObrigacao.Pendente, DateTime.UtcNow.AddDays(-5));

        _obrigacaoRepo.Setup(r => r.ObterPorEmpresaEMesAsync(_empresa.Id, 6, 2025))
            .ReturnsAsync([obrigacaoPendente, obrigacaoAtrasada]);

        var result = await _service.ObterCalendarioAsync(_empresa.Id, 6, 2025, StatusObrigacao.Pendente);

        result.Should().HaveCount(1);
        result.Single().Status.Should().Be(StatusObrigacao.Pendente);
    }

    [Fact]
    public async Task ObterCalendario_SemFiltro_DeveRetornarTodasAsObrigacoes()
    {
        var obrigacoes = new List<ObrigacaoAcessoria>
        {
            CriarObrigacao(StatusObrigacao.Pendente, DateTime.UtcNow.AddDays(10)),
            CriarObrigacao(StatusObrigacao.Entregue, DateTime.UtcNow.AddDays(-10)),
        };

        _obrigacaoRepo.Setup(r => r.ObterPorEmpresaEMesAsync(_empresa.Id, 6, 2025))
            .ReturnsAsync(obrigacoes);

        var result = await _service.ObterCalendarioAsync(_empresa.Id, 6, 2025);

        result.Should().HaveCount(2);
    }

    // ----------------------------------------------------------------
    // GerarObrigacoes
    // ----------------------------------------------------------------

    [Fact]
    public async Task GerarObrigacoes_QuandoEmpresaNaoExiste_DeveLancarKeyNotFoundException()
    {
        _empresaRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Empresa?)null);

        var act = async () => await _service.GerarObrigacoesAsync(
            new GerarObrigacoesRequest { EmpresaId = Guid.NewGuid(), Mes = 6, Ano = 2025 });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GerarObrigacoes_DeveAdicionarApenasObrigacoesNovas()
    {
        var engine = new TaxRulesEngine();
        var service = new ObrigacaoService(_obrigacaoRepo.Object, _empresaRepo.Object, engine, MapperFixture.Create());

        _empresaRepo.Setup(r => r.ObterPorIdAsync(_empresa.Id)).ReturnsAsync(_empresa);

        _obrigacaoRepo.Setup(r => r.ExisteObrigacaoAsync(
                _empresa.Id, TipoObrigacao.DAS, 3, 2025))
            .ReturnsAsync(true);

        _obrigacaoRepo.Setup(r => r.ExisteObrigacaoAsync(
                _empresa.Id, It.Is<TipoObrigacao>(t => t != TipoObrigacao.DAS), 3, 2025))
            .ReturnsAsync(false);

        _obrigacaoRepo.Setup(r => r.AdicionarAsync(It.IsAny<ObrigacaoAcessoria>()))
            .Returns(Task.CompletedTask);
        _obrigacaoRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);
        _obrigacaoRepo.Setup(r => r.ObterPorEmpresaEMesAsync(_empresa.Id, 3, 2025))
            .ReturnsAsync([]);

        await service.GerarObrigacoesAsync(new GerarObrigacoesRequest { EmpresaId = _empresa.Id, Mes = 3, Ano = 2025 });

        _obrigacaoRepo.Verify(r => r.AdicionarAsync(
            It.Is<ObrigacaoAcessoria>(o => o.Tipo == TipoObrigacao.DAS)), Times.Never);

        _obrigacaoRepo.Verify(r => r.AdicionarAsync(
            It.Is<ObrigacaoAcessoria>(o => o.Tipo == TipoObrigacao.ESocial)), Times.Once);
    }

    // ----------------------------------------------------------------
    // ObterPorId
    // ----------------------------------------------------------------

    [Fact]
    public async Task ObterPorId_QuandoExiste_DeveRetornarObrigacao()
    {
        var obrigacao = CriarObrigacao(StatusObrigacao.Pendente, DateTime.UtcNow.AddDays(5));
        _obrigacaoRepo.Setup(r => r.ObterComEntregaAsync(obrigacao.Id)).ReturnsAsync(obrigacao);

        var result = await _service.ObterPorIdAsync(obrigacao.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(obrigacao.Id);
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoExiste_DeveRetornarNull()
    {
        _obrigacaoRepo.Setup(r => r.ObterComEntregaAsync(It.IsAny<Guid>()))
            .ReturnsAsync((ObrigacaoAcessoria?)null);

        var result = await _service.ObterPorIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ----------------------------------------------------------------
    // ExportarCsv
    // ----------------------------------------------------------------

    [Fact]
    public async Task ExportarCsv_QuandoEmpresaNaoExiste_DeveLancarKeyNotFoundException()
    {
        _empresaRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Empresa?)null);

        var act = async () => await _service.ExportarCsvAsync(Guid.NewGuid(), 6, 2025);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExportarCsv_DeveRetornarBytesComConteudo()
    {
        var obrigacao = CriarObrigacao(StatusObrigacao.Pendente, DateTime.UtcNow.AddDays(5));

        _empresaRepo.Setup(r => r.ObterPorIdAsync(_empresa.Id)).ReturnsAsync(_empresa);
        _obrigacaoRepo.Setup(r => r.ObterPorEmpresaEMesAsync(_empresa.Id, 6, 2025))
            .ReturnsAsync([obrigacao]);

        var csv = await _service.ExportarCsvAsync(_empresa.Id, 6, 2025);

        csv.Should().NotBeEmpty();
        csv.Take(3).Should().Equal([0xEF, 0xBB, 0xBF]);

        var texto = System.Text.Encoding.UTF8.GetString(csv);
        texto.Should().Contain("Empresa;CNPJ");
        texto.Should().Contain("Empresa Teste");
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static ObrigacaoAcessoria CriarObrigacao(StatusObrigacao status, DateTime vencimento)
    {
        var o = new ObrigacaoAcessoria(
            _empresa.Id,
            TipoObrigacao.ESocial,
            PeriodicidadeObrigacao.Mensal,
            6, 2025,
            vencimento);

        if (status == StatusObrigacao.Entregue)
            o.MarcarComoEntregue();

        return o;
    }
}
