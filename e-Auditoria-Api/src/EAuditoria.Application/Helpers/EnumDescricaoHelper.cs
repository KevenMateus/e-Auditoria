using EAuditoria.Domain.Enums;

namespace EAuditoria.Application.Helpers;

public static class EnumDescricaoHelper
{
    public static string Descricao(this RegimeTributario regime) => regime switch
    {
        RegimeTributario.SimplesNacional  => "Simples Nacional",
        RegimeTributario.LucroPresumido   => "Lucro Presumido",
        RegimeTributario.LucroReal        => "Lucro Real",
        RegimeTributario.ImunidadeIsencao => "Imunidade / Isenção",
        _                                 => regime.ToString()
    };

    public static string Descricao(this TipoObrigacao tipo) => tipo switch
    {
        TipoObrigacao.DAS               => "DAS",
        TipoObrigacao.DEFIS             => "DEFIS",
        TipoObrigacao.DCTF              => "DCTF",
        TipoObrigacao.EFD_ICMS_IPI      => "EFD-ICMS/IPI",
        TipoObrigacao.EFD_Contribuicoes => "EFD Contribuições",
        TipoObrigacao.EFD_Reinf         => "EFD-Reinf",
        TipoObrigacao.SPED_ECD          => "SPED ECD",
        TipoObrigacao.SPED_ECF          => "SPED ECF",
        TipoObrigacao.ESocial           => "eSocial",
        TipoObrigacao.DIRF              => "DIRF",
        TipoObrigacao.RAIS              => "RAIS",
        _                               => tipo.ToString()
    };

    public static string Descricao(this StatusObrigacao status) => status switch
    {
        StatusObrigacao.Pendente     => "Pendente",
        StatusObrigacao.Atrasada     => "Atrasada",
        StatusObrigacao.Entregue     => "Entregue",
        StatusObrigacao.NaoAplicavel => "Não Aplicável",
        _                            => status.ToString()
    };
}
