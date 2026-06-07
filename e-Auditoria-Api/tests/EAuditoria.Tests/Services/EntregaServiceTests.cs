using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Application.Services;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EAuditoria.Tests.Services;

public class EntregaServiceTests
{
    private readonly Mock<IEntregaRepository> _entregaRepo = new();
    private readonly Mock<IObrigacaoService> _obrigacaoService = new();
    private readonly EntregaService _service;

    private static readonly Empresa _empresa =
        new("Empresa Teste", "11222333000181", RegimeTributario.SimplesNacional);

    public EntregaServiceTests()
    {
        _service = new EntregaService(
            _entregaRepo.Object,
            _obrigacaoService.Object,
            MapperFixture.Create());
    }

    [Fact]
    public async Task Registrar_ComObrigacaoPendente_DeveMarcarComoEntregue()
    {
        var obrigacao = CriarObrigacao(StatusObrigacao.Pendente);
        var request = new RegistrarEntregaRequest { DataEntrega = DateTime.UtcNow, Observacao = "Entregue com sucesso" };

        _obrigacaoService.Setup(s => s.ObterEntidadeComEntregaAsync(obrigacao.Id)).ReturnsAsync(obrigacao);
        _obrigacaoService.Setup(s => s.AtualizarEntidade(It.IsAny<ObrigacaoAcessoria>()));
        _entregaRepo.Setup(r => r.AdicionarAsync(It.IsAny<EntregaObrigacao>())).Returns(Task.CompletedTask);
        _entregaRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var result = await _service.RegistrarAsync(obrigacao.Id, request);

        result.Should().NotBeNull();
        result.ObrigacaoId.Should().Be(obrigacao.Id);
        result.DataEntrega.Should().BeCloseTo(request.DataEntrega, TimeSpan.FromSeconds(1));

        obrigacao.Status.Should().Be(StatusObrigacao.Entregue);

        _obrigacaoService.Verify(s => s.AtualizarEntidade(It.IsAny<ObrigacaoAcessoria>()), Times.Once);
        _entregaRepo.Verify(r => r.AdicionarAsync(It.IsAny<EntregaObrigacao>()), Times.Once);
        _entregaRepo.Verify(r => r.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Registrar_ComObrigacaoAtrasada_DeveMarcarComoEntregue()
    {
        var obrigacao = CriarObrigacao(StatusObrigacao.Pendente, DateTime.UtcNow.AddDays(-10));
        obrigacao.RecalcularStatus(DateTime.UtcNow);
        obrigacao.Status.Should().Be(StatusObrigacao.Atrasada);

        var request = new RegistrarEntregaRequest { DataEntrega = DateTime.UtcNow };

        _obrigacaoService.Setup(s => s.ObterEntidadeComEntregaAsync(obrigacao.Id)).ReturnsAsync(obrigacao);
        _obrigacaoService.Setup(s => s.AtualizarEntidade(It.IsAny<ObrigacaoAcessoria>()));
        _entregaRepo.Setup(r => r.AdicionarAsync(It.IsAny<EntregaObrigacao>())).Returns(Task.CompletedTask);
        _entregaRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var result = await _service.RegistrarAsync(obrigacao.Id, request);

        result.Should().NotBeNull();
        obrigacao.Status.Should().Be(StatusObrigacao.Entregue);
    }

    [Fact]
    public async Task Registrar_QuandoObrigacaoNaoExiste_DeveLancarKeyNotFoundException()
    {
        _obrigacaoService.Setup(s => s.ObterEntidadeComEntregaAsync(It.IsAny<Guid>()))
            .ReturnsAsync((ObrigacaoAcessoria?)null);

        var act = async () => await _service.RegistrarAsync(
            Guid.NewGuid(), new RegistrarEntregaRequest { DataEntrega = DateTime.UtcNow });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Registrar_QuandoJaEntregue_DeveLancarInvalidOperationException()
    {
        var obrigacao = CriarObrigacao(StatusObrigacao.Pendente);
        obrigacao.MarcarComoEntregue();

        _obrigacaoService.Setup(s => s.ObterEntidadeComEntregaAsync(obrigacao.Id)).ReturnsAsync(obrigacao);

        var act = async () => await _service.RegistrarAsync(
            obrigacao.Id, new RegistrarEntregaRequest { DataEntrega = DateTime.UtcNow });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já foi registrada*");
    }

    [Fact]
    public async Task Registrar_ComObservacao_DevePersistirObservacao()
    {
        var obrigacao = CriarObrigacao(StatusObrigacao.Pendente);
        var request = new RegistrarEntregaRequest { DataEntrega = DateTime.UtcNow, Observacao = "Observação de teste" };

        _obrigacaoService.Setup(s => s.ObterEntidadeComEntregaAsync(obrigacao.Id)).ReturnsAsync(obrigacao);
        _obrigacaoService.Setup(s => s.AtualizarEntidade(It.IsAny<ObrigacaoAcessoria>()));
        _entregaRepo.Setup(r => r.AdicionarAsync(It.IsAny<EntregaObrigacao>())).Returns(Task.CompletedTask);
        _entregaRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var result = await _service.RegistrarAsync(obrigacao.Id, request);

        result.Observacao.Should().Be("Observação de teste");
    }

    // ----------------------------------------------------------------
    // ObterHistoricoAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ObterHistorico_DeveRetornarEntregasDaEmpresa()
    {
        var empresaId = Guid.NewGuid();
        var obrigacao = CriarObrigacao(StatusObrigacao.Entregue);
        var entregas = new List<EntregaObrigacao>
        {
            new(obrigacao.Id, DateTime.UtcNow.AddDays(-3), "Entregue"),
            new(obrigacao.Id, DateTime.UtcNow.AddDays(-1), null),
        };

        _entregaRepo.Setup(r => r.ObterHistoricoPorEmpresaAsync(empresaId)).ReturnsAsync(entregas);

        var result = await _service.ObterHistoricoAsync(empresaId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterHistorico_SemEntregas_DeveRetornarListaVazia()
    {
        _entregaRepo.Setup(r => r.ObterHistoricoPorEmpresaAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var result = await _service.ObterHistoricoAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static ObrigacaoAcessoria CriarObrigacao(
        StatusObrigacao status,
        DateTime? vencimento = null)
    {
        var o = new ObrigacaoAcessoria(
            _empresa.Id,
            TipoObrigacao.DAS,
            PeriodicidadeObrigacao.Mensal,
            6, 2025,
            vencimento ?? DateTime.UtcNow.AddDays(10));

        if (status == StatusObrigacao.Entregue)
            o.MarcarComoEntregue();

        return o;
    }
}
