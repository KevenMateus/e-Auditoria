using BCrypt.Net;
using EAuditoria.Application.Engine;
using EAuditoria.Domain.Entities;
using EAuditoria.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EAuditoria.Infrastructure.Data.Seed;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly ITaxRulesEngine _taxRulesEngine;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        ITaxRulesEngine taxRulesEngine,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _taxRulesEngine = taxRulesEngine;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var hoje = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        if (!await _context.Usuarios.AnyAsync())
        {
            _logger.LogInformation("Criando usuário admin padrão...");
            var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@2025");
            var admin = new Usuario("Administrador", "admin@eauditoria.com.br", adminHash, "Admin");
            await _context.Usuarios.AddAsync(admin);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Usuário admin criado: admin@eauditoria.com.br / Admin@2025");
        }

        List<Empresa> empresas;
        if (!await _context.Empresas.AnyAsync())
        {
            _logger.LogInformation("Criando empresas de demonstração...");
            empresas = CriarEmpresas();
            await _context.Empresas.AddRangeAsync(empresas);
            await _context.SaveChangesAsync();
        }
        else
        {
            empresas = await _context.Empresas.ToListAsync();
            _logger.LogInformation("Empresas já existem ({Count}). Verificando obrigações...", empresas.Count);
        }

        if (!await _context.ObrigacoesAcessorias.AnyAsync())
        {
            _logger.LogInformation("Gerando obrigações de demonstração...");
            var obrigacoes = GerarObrigacoesDemo(empresas, hoje);
            await _context.ObrigacoesAcessorias.AddRangeAsync(obrigacoes);
            await _context.SaveChangesAsync();

            await SeedEntregasAsync(obrigacoes, hoje);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seed concluído: {Empresas} empresas, {Obrigacoes} obrigações.",
                empresas.Count, obrigacoes.Count);
        }
        else
        {
            _logger.LogInformation("Obrigações já existem. Seed ignorado.");
        }
    }

    private static List<Empresa> CriarEmpresas() =>
    [
        new Empresa("Padaria Pão Quente Ltda",       "11222333000181", RegimeTributario.SimplesNacional),
        new Empresa("Tech Solutions ME",             "22333444000195", RegimeTributario.SimplesNacional),
        new Empresa("Consultoria Alpha Ltda",        "33444555000109", RegimeTributario.LucroPresumido),
        new Empresa("Distribuidora Beta S/A",        "44555666000117", RegimeTributario.LucroPresumido),
        new Empresa("Banco Meridional S/A",          "55666777000126", RegimeTributario.LucroReal),
        new Empresa("Indústria Gama S/A",            "66777888000134", RegimeTributario.LucroReal),
        new Empresa("Associação Cultural Delta",     "77888999000148", RegimeTributario.ImunidadeIsencao),
        new Empresa("Serviços Epsilon ME",           "88999000000156", RegimeTributario.SimplesNacional),
        new Empresa("Comércio Zeta Ltda",            "99000111000163", RegimeTributario.LucroPresumido),
        new Empresa("Holding Eta Participações S/A", "10111222000171", RegimeTributario.LucroReal),
    ];

    private List<ObrigacaoAcessoria> GerarObrigacoesDemo(List<Empresa> empresas, DateTime hoje)
    {
        var obrigacoes = new List<ObrigacaoAcessoria>();

        var periodos = new List<(int Ano, int Mes)>
        {
            (hoje.AddMonths(-3).Year, hoje.AddMonths(-3).Month),
            (hoje.AddMonths(-2).Year, hoje.AddMonths(-2).Month),
            (hoje.AddMonths(-1).Year, hoje.AddMonths(-1).Month),
            (hoje.Year, hoje.Month),
            (hoje.AddMonths(1).Year,  hoje.AddMonths(1).Month),
            (hoje.AddMonths(2).Year,  hoje.AddMonths(2).Month),
            (hoje.Year, 1),
        };

        foreach (var (ano, mes) in periodos.DistinctBy(p => (p.Ano, p.Mes)))
            foreach (var empresa in empresas)
                obrigacoes.AddRange(_taxRulesEngine.GerarObrigacoes(empresa, mes, ano));

        return obrigacoes;
    }

    private async Task SeedEntregasAsync(List<ObrigacaoAcessoria> obrigacoes, DateTime hoje)
    {
        var anteriores = obrigacoes
            .Where(o => o.AnoCompetencia < hoje.Year ||
                       (o.AnoCompetencia == hoje.Year && o.Competencia < hoje.Month))
            .ToList();

        var rng = new Random(42);

        foreach (var obrigacao in anteriores)
        {
            if (rng.NextDouble() >= 0.6) continue;

            obrigacao.MarcarComoEntregue();

            await _context.EntregasObrigacoes.AddAsync(new EntregaObrigacao(
                obrigacao.Id,
                obrigacao.Vencimento.AddDays(-rng.Next(1, 6)),
                "Entrega automática de demonstração"));
        }
    }
}
