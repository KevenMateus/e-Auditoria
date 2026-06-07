using System.Text;
using AutoMapper;
using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Engine;
using EAuditoria.Application.Helpers;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Interfaces;

namespace EAuditoria.Application.Services;

public class ObrigacaoService : IObrigacaoService
{
    private readonly IObrigacaoRepository _obrigacaoRepository;

    // IEmpresaRepository é usado apenas para buscar dados da empresa necessários
    // ao gerar obrigações e exportar CSV — dentro do mesmo bounded context.
    private readonly IEmpresaRepository _empresaRepository;

    private readonly ITaxRulesEngine _taxRulesEngine;
    private readonly IMapper _mapper;

    public ObrigacaoService(
        IObrigacaoRepository obrigacaoRepository,
        IEmpresaRepository empresaRepository,
        ITaxRulesEngine taxRulesEngine,
        IMapper mapper)
    {
        _obrigacaoRepository = obrigacaoRepository;
        _empresaRepository = empresaRepository;
        _taxRulesEngine = taxRulesEngine;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ObrigacaoResponse>> ObterCalendarioAsync(
        Guid empresaId, int mes, int ano, StatusObrigacao? filtroStatus = null)
    {
        var obrigacoes = await _obrigacaoRepository.ObterPorEmpresaEMesAsync(empresaId, mes, ano);

        var hoje = DateTime.UtcNow;
        foreach (var o in obrigacoes)
            o.RecalcularStatus(hoje);

        if (filtroStatus.HasValue)
            obrigacoes = obrigacoes.Where(o => o.Status == filtroStatus.Value);

        return _mapper.Map<IEnumerable<ObrigacaoResponse>>(obrigacoes);
    }

    public async Task<IEnumerable<ObrigacaoResponse>> GerarObrigacoesAsync(GerarObrigacoesRequest request)
    {
        var empresa = await _empresaRepository.ObterPorIdAsync(request.EmpresaId)
            ?? throw new KeyNotFoundException($"Empresa '{request.EmpresaId}' não encontrada.");

        var novas = _taxRulesEngine
            .GerarObrigacoes(empresa, request.Mes, request.Ano)
            .ToList();

        var persistidas = new List<ObrigacaoAcessoria>();

        foreach (var obrigacao in novas)
        {
            var jaExiste = await _obrigacaoRepository.ExisteObrigacaoAsync(
                empresa.Id, obrigacao.Tipo, request.Mes, request.Ano);

            if (!jaExiste)
            {
                await _obrigacaoRepository.AdicionarAsync(obrigacao);
                persistidas.Add(obrigacao);
            }
        }

        if (persistidas.Count > 0)
            await _obrigacaoRepository.SalvarAsync();

        var resultado = await _obrigacaoRepository.ObterPorEmpresaEMesAsync(empresa.Id, request.Mes, request.Ano);

        var hoje = DateTime.UtcNow;
        foreach (var o in resultado)
            o.RecalcularStatus(hoje);

        return _mapper.Map<IEnumerable<ObrigacaoResponse>>(resultado);
    }

    public async Task GerarParaEmpresaAsync(Empresa empresa, int mes, int ano)
    {
        var geradas = _taxRulesEngine.GerarObrigacoes(empresa, mes, ano);
        foreach (var obrigacao in geradas)
        {
            var jaExiste = await _obrigacaoRepository.ExisteObrigacaoAsync(
                empresa.Id, obrigacao.Tipo, mes, ano);

            if (!jaExiste)
                await _obrigacaoRepository.AdicionarAsync(obrigacao);
        }
    }

    public Task SalvarAsync() => _obrigacaoRepository.SalvarAsync();

    public async Task<ObrigacaoResponse?> ObterPorIdAsync(Guid id)
    {
        var obrigacao = await _obrigacaoRepository.ObterComEntregaAsync(id);
        return obrigacao is null ? null : _mapper.Map<ObrigacaoResponse>(obrigacao);
    }

    public Task<ObrigacaoAcessoria?> ObterEntidadeComEntregaAsync(Guid obrigacaoId) =>
        _obrigacaoRepository.ObterComEntregaAsync(obrigacaoId);

    public void AtualizarEntidade(ObrigacaoAcessoria obrigacao) =>
        _obrigacaoRepository.Atualizar(obrigacao);

    public Task<DashboardCounts> ObterContagensDashboardAsync(int mes, int ano) =>
        _obrigacaoRepository.ObterContagensDashboardAsync(mes, ano);

    public async Task<IEnumerable<AlertaObrigacaoResponse>> ObterVencendoEmDiasAsync(int dias)
    {
        var hoje = DateTime.UtcNow;
        var obrigacoes = await _obrigacaoRepository.ObterVencendoEmDiasAsync(dias);
        return obrigacoes.Select(o => MapAlerta(o, hoje));
    }

    public async Task<IEnumerable<AlertaObrigacaoResponse>> ObterAtrasadasAsync()
    {
        var hoje = DateTime.UtcNow;
        var obrigacoes = await _obrigacaoRepository.ObterAtrasadasAsync();
        return obrigacoes.Select(o => MapAlerta(o, hoje));
    }

    public async Task<byte[]> ExportarCsvAsync(Guid empresaId, int mes, int ano)
    {
        var empresa = await _empresaRepository.ObterPorIdAsync(empresaId)
            ?? throw new KeyNotFoundException($"Empresa '{empresaId}' não encontrada.");

        var obrigacoes = await _obrigacaoRepository.ObterPorEmpresaEMesAsync(empresaId, mes, ano);

        var hoje = DateTime.UtcNow;
        foreach (var o in obrigacoes)
            o.RecalcularStatus(hoje);

        var sb = new StringBuilder();
        sb.AppendLine("Empresa;CNPJ;Regime;Obrigação;Periodicidade;Competência;Ano;Vencimento;Status;Data Entrega;Observação");

        foreach (var o in obrigacoes.OrderBy(x => x.Vencimento))
        {
            var dataEntrega = o.Entrega?.DataEntrega.ToString("dd/MM/yyyy") ?? "";
            var observacao = o.Entrega?.Observacao?.Replace(";", ",") ?? "";

            sb.AppendLine(string.Join(";",
                empresa.RazaoSocial,
                empresa.Cnpj,
                empresa.RegimeTributario.Descricao(),
                o.Tipo.Descricao(),
                o.Periodicidade == PeriodicidadeObrigacao.Mensal ? "Mensal" : "Anual",
                o.Competencia.ToString(),
                o.AnoCompetencia.ToString(),
                o.Vencimento.ToString("dd/MM/yyyy"),
                o.Status.Descricao(),
                dataEntrega,
                observacao));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static AlertaObrigacaoResponse MapAlerta(ObrigacaoAcessoria o, DateTime hoje) =>
        new()
        {
            ObrigacaoId     = o.Id,
            EmpresaId       = o.EmpresaId,
            EmpresaNome     = o.Empresa?.RazaoSocial ?? string.Empty,
            Cnpj            = o.Empresa?.Cnpj ?? string.Empty,
            Tipo            = o.Tipo,
            TipoDescricao   = o.Tipo.Descricao(),
            Vencimento      = o.Vencimento,
            DiasRestantes   = (int)(o.Vencimento.Date - hoje.Date).TotalDays,
            Status          = o.Status,
            StatusDescricao = o.Status.Descricao()
        };
}
