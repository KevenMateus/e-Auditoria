namespace EAuditoria.Application.DTOs.Response;

/// <summary>Visão consolidada do painel de obrigações para um mês/ano de referência.</summary>
public class DashboardResponse
{
    /// <summary>Total de empresas ativas cadastradas no sistema.</summary>
    /// <example>40</example>
    public int TotalEmpresas { get; set; }

    /// <summary>Total de obrigações geradas para o mês/ano de referência.</summary>
    /// <example>120</example>
    public int ObrigacoesMes { get; set; }

    /// <summary>Obrigações com vencimento futuro ainda não entregues.</summary>
    /// <example>75</example>
    public int Pendentes { get; set; }

    /// <summary>Obrigações registradas como entregues no mês/ano de referência.</summary>
    /// <example>35</example>
    public int Entregues { get; set; }

    /// <summary>Obrigações cujo vencimento já passou sem entrega registrada.</summary>
    /// <example>10</example>
    public int Atrasadas { get; set; }

    /// <summary>Mês de referência (1–12).</summary>
    /// <example>6</example>
    public int Mes { get; set; }

    /// <summary>Ano de referência.</summary>
    /// <example>2025</example>
    public int Ano { get; set; }
}
