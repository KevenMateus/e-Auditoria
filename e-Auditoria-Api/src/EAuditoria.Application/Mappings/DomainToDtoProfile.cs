using AutoMapper;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Helpers;
using EAuditoria.Domain.Entities;

namespace EAuditoria.Application.Mappings;

public class DomainToDtoProfile : Profile
{
    public DomainToDtoProfile()
    {
        CreateMap<Empresa, EmpresaResponse>()
            .ForMember(dest => dest.RegimeTributarioDescricao,
                opt => opt.MapFrom(src => src.RegimeTributario.Descricao()));

        CreateMap<ObrigacaoAcessoria, ObrigacaoResponse>()
            .ForMember(dest => dest.EmpresaNome,
                opt => opt.MapFrom(src => src.Empresa != null ? src.Empresa.RazaoSocial : string.Empty))
            .ForMember(dest => dest.TipoDescricao,
                opt => opt.MapFrom(src => src.Tipo.Descricao()))
            .ForMember(dest => dest.StatusDescricao,
                opt => opt.MapFrom(src => src.Status.Descricao()))
            .ForMember(dest => dest.Entrega,
                opt => opt.MapFrom(src => src.Entrega));

        CreateMap<EntregaObrigacao, EntregaResponse>();
    }
}
