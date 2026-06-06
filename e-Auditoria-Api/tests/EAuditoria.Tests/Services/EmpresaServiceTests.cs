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

public class EmpresaServiceTests
{
    private readonly Mock<IEmpresaRepository> _empresaRepo = new();
    private readonly Mock<IObrigacaoRepository> _obrigacaoRepo = new();
    private readonly Mock<ITaxRulesEngine> _engine = new();
    private readonly EmpresaService _service;

    public EmpresaServiceTests()
    {
        _service = new EmpresaService(
            _empresaRepo.Object,
            _obrigacaoRepo.Object,
            _engine.Object,
            MapperFixture.Create());

        _engine.Setup(e => e.GerarObrigacoes(It.IsAny<Empresa>(), It.IsAny<int>(), It.IsAny<int>()))
               .Returns([]);

        _obrigacaoRepo.Setup(r => r.ExisteObrigacaoAsync(
                It.IsAny<Guid>(), It.IsAny<TipoObrigacao>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        _obrigacaoRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(0);
    }

    // ----------------------------------------------------------------
    // Listar
    // ----------------------------------------------------------------

    [Fact]
    public async Task Listar_DeveRetornarEmpresasAtivas()
    {
        var empresas = new List<Empresa>
        {
            new("Empresa A", "11222333000181", RegimeTributario.SimplesNacional),
            new("Empresa B", "22333444000195", RegimeTributario.LucroReal),
        };

        _empresaRepo.Setup(r => r.ObterAtivasAsync()).ReturnsAsync(empresas);

        var result = (await _service.ListarAsync()).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(e => e.RazaoSocial == "Empresa A");
        result.Should().Contain(e => e.RazaoSocial == "Empresa B");
    }

    [Fact]
    public async Task Listar_QuandoNenhumaEmpresa_DeveRetornarListaVazia()
    {
        _empresaRepo.Setup(r => r.ObterAtivasAsync()).ReturnsAsync([]);

        var result = await _service.ListarAsync();

        result.Should().BeEmpty();
    }

    // ----------------------------------------------------------------
    // ObterPorId
    // ----------------------------------------------------------------

    [Fact]
    public async Task ObterPorId_QuandoExiste_DeveRetornarEmpresa()
    {
        var empresa = new Empresa("Empresa Teste", "11222333000181", RegimeTributario.LucroPresumido);
        _empresaRepo.Setup(r => r.ObterPorIdAsync(empresa.Id)).ReturnsAsync(empresa);

        var result = await _service.ObterPorIdAsync(empresa.Id);

        result.Should().NotBeNull();
        result!.RazaoSocial.Should().Be("Empresa Teste");
        result.RegimeTributario.Should().Be(RegimeTributario.LucroPresumido);
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoExiste_DeveRetornarNull()
    {
        _empresaRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Empresa?)null);

        var result = await _service.ObterPorIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ----------------------------------------------------------------
    // Criar
    // ----------------------------------------------------------------

    [Fact]
    public async Task Criar_ComDadosValidos_DeveCriarERetornarEmpresa()
    {
        var request = new CriarEmpresaRequest
        {
            RazaoSocial = "Nova Empresa",
            Cnpj = "11.222.333/0001-81",
            RegimeTributario = RegimeTributario.SimplesNacional
        };

        _empresaRepo.Setup(r => r.ExisteCnpjAsync("11222333000181", It.IsAny<Guid?>()))
            .ReturnsAsync(false);
        _empresaRepo.Setup(r => r.AdicionarAsync(It.IsAny<Empresa>())).Returns(Task.CompletedTask);
        _empresaRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var result = await _service.CriarAsync(request);

        result.Should().NotBeNull();
        result.RazaoSocial.Should().Be("Nova Empresa");
        result.Cnpj.Should().Be("11222333000181");
        result.RegimeTributario.Should().Be(RegimeTributario.SimplesNacional);
        result.Ativo.Should().BeTrue();

        _empresaRepo.Verify(r => r.AdicionarAsync(It.IsAny<Empresa>()), Times.Once);
        _empresaRepo.Verify(r => r.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Criar_ComCnpjFormatado_DeveLimparFormatacao()
    {
        var request = new CriarEmpresaRequest
        {
            RazaoSocial = "Empresa X",
            Cnpj = "22.333.444/0001-95",
            RegimeTributario = RegimeTributario.LucroReal
        };

        _empresaRepo.Setup(r => r.ExisteCnpjAsync("22333444000195", It.IsAny<Guid?>())).ReturnsAsync(false);
        _empresaRepo.Setup(r => r.AdicionarAsync(It.IsAny<Empresa>())).Returns(Task.CompletedTask);
        _empresaRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var result = await _service.CriarAsync(request);

        result.Cnpj.Should().Be("22333444000195");
    }

    [Fact]
    public async Task Criar_ComCnpjDuplicado_DeveLancarInvalidOperationException()
    {
        var request = new CriarEmpresaRequest
        {
            RazaoSocial = "Empresa Y",
            Cnpj = "11222333000181",
            RegimeTributario = RegimeTributario.LucroPresumido
        };

        _empresaRepo.Setup(r => r.ExisteCnpjAsync("11222333000181", It.IsAny<Guid?>())).ReturnsAsync(true);

        var act = async () => await _service.CriarAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já cadastrado*");
    }

    [Fact]
    public async Task Criar_DeveGerarObrigacoesAutomaticamente()
    {
        var request = new CriarEmpresaRequest
        {
            RazaoSocial = "Empresa Z",
            Cnpj = "11222333000181",
            RegimeTributario = RegimeTributario.SimplesNacional
        };

        _empresaRepo.Setup(r => r.ExisteCnpjAsync(It.IsAny<string>(), It.IsAny<Guid?>())).ReturnsAsync(false);
        _empresaRepo.Setup(r => r.AdicionarAsync(It.IsAny<Empresa>())).Returns(Task.CompletedTask);
        _empresaRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        await _service.CriarAsync(request);

        _engine.Verify(e => e.GerarObrigacoes(It.IsAny<Empresa>(), It.IsAny<int>(), It.IsAny<int>()),
            Times.AtLeast(12));
    }

    // ----------------------------------------------------------------
    // Atualizar
    // ----------------------------------------------------------------

    [Fact]
    public async Task Atualizar_ComEmpresaExistente_DeveAtualizarERetornar()
    {
        var empresa = new Empresa("Empresa Original", "11222333000181", RegimeTributario.SimplesNacional);
        var request = new AtualizarEmpresaRequest
        {
            RazaoSocial = "Empresa Atualizada",
            RegimeTributario = RegimeTributario.LucroReal
        };

        _empresaRepo.Setup(r => r.ObterPorIdAsync(empresa.Id)).ReturnsAsync(empresa);
        _empresaRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        var result = await _service.AtualizarAsync(empresa.Id, request);

        result.RazaoSocial.Should().Be("Empresa Atualizada");
        result.RegimeTributario.Should().Be(RegimeTributario.LucroReal);
        _empresaRepo.Verify(r => r.Atualizar(It.IsAny<Empresa>()), Times.Once);
    }

    [Fact]
    public async Task Atualizar_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        _empresaRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Empresa?)null);

        var act = async () => await _service.AtualizarAsync(Guid.NewGuid(),
            new AtualizarEmpresaRequest { RazaoSocial = "X", RegimeTributario = RegimeTributario.LucroReal });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ----------------------------------------------------------------
    // Remover
    // ----------------------------------------------------------------

    [Fact]
    public async Task Remover_ComEmpresaExistente_DeveDesativar()
    {
        var empresa = new Empresa("Empresa D", "11222333000181", RegimeTributario.LucroReal);
        _empresaRepo.Setup(r => r.ObterPorIdAsync(empresa.Id)).ReturnsAsync(empresa);
        _empresaRepo.Setup(r => r.SalvarAsync()).ReturnsAsync(1);

        await _service.RemoverAsync(empresa.Id);

        empresa.Ativo.Should().BeFalse();
        _empresaRepo.Verify(r => r.Atualizar(It.IsAny<Empresa>()), Times.Once);
    }

    [Fact]
    public async Task Remover_QuandoNaoExiste_DeveLancarKeyNotFoundException()
    {
        _empresaRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>())).ReturnsAsync((Empresa?)null);

        var act = async () => await _service.RemoverAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
