using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Response;

public class EmpresaResponse
{
    public Guid Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public RegimeTributario RegimeTributario { get; set; }
    public string RegimeTributarioDescricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
