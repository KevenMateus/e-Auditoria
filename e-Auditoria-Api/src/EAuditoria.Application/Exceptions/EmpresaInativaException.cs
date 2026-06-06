namespace EAuditoria.Application.Exceptions;

/// <summary>
/// Lançada quando se tenta cadastrar um CNPJ que já pertence a uma empresa inativa.
/// Carrega o Id da empresa inativa para permitir reativação direta pelo cliente.
/// </summary>
public class EmpresaInativaException : Exception
{
    public Guid EmpresaInativaId { get; }
    public string RazaoSocial { get; }

    public EmpresaInativaException(Guid empresaInativaId, string razaoSocial, string cnpj)
        : base($"O CNPJ '{cnpj}' pertence à empresa '{razaoSocial}', que está inativa.")
    {
        EmpresaInativaId = empresaInativaId;
        RazaoSocial = razaoSocial;
    }
}
