using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Request;

public class CriarEmpresaRequest
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public RegimeTributario RegimeTributario { get; set; }
}
