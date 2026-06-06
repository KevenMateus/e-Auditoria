using AutoMapper;
using EAuditoria.Application.Mappings;

namespace EAuditoria.Tests.Services;

/// <summary>
/// Cria um IMapper real com os profiles de produção.
/// Evita mockar o mapper — mapear é lógica que deve funcionar de verdade.
/// </summary>
public static class MapperFixture
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new DomainToDtoProfile());
        });

        config.AssertConfigurationIsValid();
        return config.CreateMapper();
    }
}
