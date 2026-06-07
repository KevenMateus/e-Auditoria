using AutoMapper;
using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Exceptions;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Interfaces;

namespace EAuditoria.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IObrigacaoService _obrigacaoService;
    private readonly IMapper _mapper;

    public EmpresaService(
        IEmpresaRepository empresaRepository,
        IObrigacaoService obrigacaoService,
        IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _obrigacaoService = obrigacaoService;
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

    public async Task<Empresa?> ObterEntidadePorIdAsync(Guid id) =>
        await _empresaRepository.ObterPorIdAsync(id);

    public async Task<EmpresaResponse> CriarAsync(CriarEmpresaRequest request)
    {
        var cnpjLimpo = LimparCnpj(request.Cnpj);

        var existente = await _empresaRepository.ObterPorCnpjAsync(cnpjLimpo);
        if (existente is not null)
        {
            if (existente.Ativo)
                throw new InvalidOperationException($"CNPJ '{request.Cnpj}' já está cadastrado em uma empresa ativa.");

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

        // Garante que janeiro (obrigações anuais) sempre seja incluído
        periodos.Add((hoje.Year, 1));

        foreach (var (ano, mes) in periodos.DistinctBy(p => (p.Ano, p.Mes)))
            await _obrigacaoService.GerarParaEmpresaAsync(empresa, mes, ano);

        await _obrigacaoService.SalvarAsync();
    }

    private static string LimparCnpj(string cnpj) =>
        new string(cnpj.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}
