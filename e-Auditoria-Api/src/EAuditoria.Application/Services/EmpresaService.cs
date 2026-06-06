using AutoMapper;
using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Engine;
using EAuditoria.Application.Exceptions;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Interfaces;

namespace EAuditoria.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IObrigacaoRepository _obrigacaoRepository;
    private readonly ITaxRulesEngine _taxRulesEngine;
    private readonly IMapper _mapper;

    public EmpresaService(
        IEmpresaRepository empresaRepository,
        IObrigacaoRepository obrigacaoRepository,
        ITaxRulesEngine taxRulesEngine,
        IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _obrigacaoRepository = obrigacaoRepository;
        _taxRulesEngine = taxRulesEngine;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EmpresaResponse>> ListarAsync()
    {
        var empresas = await _empresaRepository.ObterAtivasAsync();
        return _mapper.Map<IEnumerable<EmpresaResponse>>(empresas);
    }

    public async Task<IEnumerable<EmpresaResponse>> ListarInativasAsync()
    {
        var empresas = await _empresaRepository.ObterInativasAsync();
        return _mapper.Map<IEnumerable<EmpresaResponse>>(empresas);
    }

    public async Task<EmpresaResponse?> ObterPorIdAsync(Guid id)
    {
        var empresa = await _empresaRepository.ObterPorIdAsync(id);
        return empresa is null ? null : _mapper.Map<EmpresaResponse>(empresa);
    }

    public async Task<EmpresaResponse> CriarAsync(CriarEmpresaRequest request)
    {
        var cnpjLimpo = LimparCnpj(request.Cnpj);

        // Verifica se o CNPJ já existe (ativa ou inativa)
        var existente = await _empresaRepository.ObterPorCnpjAsync(cnpjLimpo);
        if (existente is not null)
        {
            if (existente.Ativo)
                throw new InvalidOperationException($"CNPJ '{request.Cnpj}' já está cadastrado em uma empresa ativa.");

            // CNPJ pertence a empresa inativa — informa ao cliente para que possa reativar
            throw new EmpresaInativaException(existente.Id, existente.RazaoSocial, request.Cnpj);
        }

        var empresa = new Empresa(request.RazaoSocial, cnpjLimpo, request.RegimeTributario);
        await _empresaRepository.AdicionarAsync(empresa);
        await _empresaRepository.SalvarAsync();
        await GerarObrigacoesIniciais(empresa);

        return _mapper.Map<EmpresaResponse>(empresa);
    }

    public async Task<EmpresaResponse> AtualizarAsync(Guid id, AtualizarEmpresaRequest request)
    {
        var empresa = await _empresaRepository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Empresa '{id}' não encontrada.");

        empresa.Atualizar(request.RazaoSocial, request.RegimeTributario);
        _empresaRepository.Atualizar(empresa);
        await _empresaRepository.SalvarAsync();

        return _mapper.Map<EmpresaResponse>(empresa);
    }

    public async Task RemoverAsync(Guid id)
    {
        var empresa = await _empresaRepository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Empresa '{id}' não encontrada.");

        empresa.Desativar();
        _empresaRepository.Atualizar(empresa);
        await _empresaRepository.SalvarAsync();
    }

    public async Task<EmpresaResponse> ReativarAsync(Guid id)
    {
        var empresa = await _empresaRepository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Empresa '{id}' não encontrada.");

        if (empresa.Ativo)
            throw new InvalidOperationException($"Empresa '{empresa.RazaoSocial}' já está ativa.");

        empresa.Reativar();
        _empresaRepository.Atualizar(empresa);
        await _empresaRepository.SalvarAsync();

        // Gera obrigações dos próximos 12 meses (empresa volta do zero)
        await GerarObrigacoesIniciais(empresa);

        return _mapper.Map<EmpresaResponse>(empresa);
    }

    private async Task GerarObrigacoesIniciais(Empresa empresa)
    {
        var hoje = DateTime.UtcNow;
        var periodos = new List<(int Ano, int Mes)>();

        for (int i = 0; i < 12; i++)
        {
            var data = hoje.AddMonths(i);
            periodos.Add((data.Year, data.Month));
        }

        periodos.Add((hoje.Year, 1));

        foreach (var (ano, mes) in periodos.DistinctBy(p => (p.Ano, p.Mes)))
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

        await _obrigacaoRepository.SalvarAsync();
    }

    private static string LimparCnpj(string cnpj) =>
        new string(cnpj.Where(char.IsDigit).ToArray());
}
