using AutoMapper;
using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using EAuditoria.Domain.Interfaces;

namespace EAuditoria.Application.Services;

public class EntregaService : IEntregaService
{
    private readonly IEntregaRepository _entregaRepository;
    private readonly IObrigacaoService _obrigacaoService;
    private readonly IMapper _mapper;

    public EntregaService(
        IEntregaRepository entregaRepository,
        IObrigacaoService obrigacaoService,
        IMapper mapper)
    {
        _entregaRepository = entregaRepository;
        _obrigacaoService = obrigacaoService;
        _mapper = mapper;
    }

    public async Task<EntregaResponse> RegistrarAsync(Guid obrigacaoId, RegistrarEntregaRequest request)
    {
        var obrigacao = await _obrigacaoService.ObterEntidadeComEntregaAsync(obrigacaoId)
            ?? throw new KeyNotFoundException($"Obrigação '{obrigacaoId}' não encontrada.");

        if (obrigacao.Status == StatusObrigacao.Entregue)
            throw new InvalidOperationException("Esta obrigação já foi registrada como entregue.");

        var entrega = new EntregaObrigacao(obrigacaoId, request.DataEntrega, request.Observacao);

        obrigacao.MarcarComoEntregue();
        _obrigacaoService.AtualizarEntidade(obrigacao);

        await _entregaRepository.AdicionarAsync(entrega);
        await _entregaRepository.SalvarAsync();

        return _mapper.Map<EntregaResponse>(entrega);
    }

    public async Task<IEnumerable<EntregaResponse>> ObterHistoricoAsync(Guid empresaId)
    {
        var entregas = await _entregaRepository.ObterHistoricoPorEmpresaAsync(empresaId);
        return _mapper.Map<IEnumerable<EntregaResponse>>(entregas);
    }
}
