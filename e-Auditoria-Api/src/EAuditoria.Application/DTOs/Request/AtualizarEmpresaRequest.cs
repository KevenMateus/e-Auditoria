using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.DTOs.Request;

public class AtualizarEmpresaRequest
{
    public string RazaoSocial { get; set; } = string.Empty;
    public RegimeTributario RegimeTributario { get; set; }
}
