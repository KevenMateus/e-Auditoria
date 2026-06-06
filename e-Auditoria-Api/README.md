# e-Auditoria API — Backend

API RESTful em **.NET 9** para o Painel de Obrigações Acessórias da e-Auditoria.  
Gerencia empresas, calendário fiscal, entregas e alertas de vencimento.

---

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Runtime | .NET 9 / ASP.NET Core Minimal APIs |
| ORM | Entity Framework Core 9 |
| Banco | PostgreSQL 16 |
| Logs | Serilog (Console sink) |
| Docs | Swagger / OpenAPI (Swashbuckle) |
| Testes | xUnit + FluentAssertions + Moq |
| Container | Docker + Docker Compose |

---

## Arquitetura

Adotei **Clean Architecture** com quatro camadas explícitas:

```
src/
├── EAuditoria.Domain/          # Entidades, enums, interfaces de repositório
├── EAuditoria.Application/     # DTOs, serviços, AutoMapper profiles, TaxRulesEngine
├── EAuditoria.Infrastructure/  # EF Core, repositórios, migrations, seed
└── EAuditoria.API/             # Endpoints (Minimal APIs), middleware, DI, Program.cs
tests/
└── EAuditoria.Tests/           # Testes unitários (engine + services)
```

### Por que Clean Architecture?

O domínio fiscal é rico em regras — a `TaxRulesEngine` precisa ser testável sem nenhuma dependência de banco ou HTTP. Separar Domain/Application de Infrastructure garante que os testes unitários rodem sem banco real e que a engine de regras possa evoluir sem tocar no EF Core.

### Decisões técnicas

**Minimal APIs (.NET 9)**  
Todos os endpoints são registrados como delegates em `Program.cs` via grupos (`MapGroup`) com extensões estáticas por recurso. Cada arquivo em `Endpoints/` exporta um método de extensão sobre `RouteGroupBuilder`, mantendo a separação de responsabilidades sem a cerimônia de `ControllerBase`. A autenticação usa `.RequireAuthorization()` no grupo, e o Swagger é alimentado por `.WithTags()`, `.WithSummary()` e `.WithOpenApi()` em cada endpoint.

**DTOs como `class` com `get; set;`**  
Garante compatibilidade com System.Text.Json sem configurações adicionais e segue o padrão de inicialização por propriedade, igual às entidades de domínio.

**`DateTime` sempre `Kind=Utc`**  
Npgsql exige `DateTimeKind.Utc` para colunas `timestamp with time zone`. Toda criação de `DateTime` usa `DateTime.UtcNow` ou o helper `Utc(ano, mes, dia)` na `TaxRulesEngine`. Nunca `DateTime.UtcNow.Date` (que retornaria `Kind=Unspecified`).

**Seed dois-fases**  
O `DatabaseSeeder` verifica empresas e obrigações de forma independente. Se o container morreu após criar as empresas mas antes de criar as obrigações, a próxima inicialização completa o seed sem duplicar dados.

**Injeção de dependências separada por responsabilidade**  
Pasta `Dependencies/` com classes estáticas especializadas (`InfrastructureDependencies`, `ServiceDependencies`, `RepositoryDependencies`, `SwaggerDependencies`, `CorsDependencies`) ao invés de um único `Startup.cs` monolítico.

**Índice único `(empresa_id, tipo, competencia, ano_competencia)`**  
Garante na camada de banco que uma empresa nunca tenha a mesma obrigação duplicada para o mesmo período — sem depender de lógica na aplicação.

---

## Engine de Regras Tributárias

`TaxRulesEngine` é um `sealed class` registrado como Singleton (sem estado mutável).

Regimes suportados: `SimplesNacional`, `LucroPresumido`, `LucroReal`, `ImunidadeIsencao`.

| Obrigação | Periodicidade | Regimes | Vencimento |
|-----------|-------------|---------|-----------|
| DAS | Mensal | Simples | Dia 20 mês seguinte (prorrogado se fim de semana) |
| DCTF | Mensal | LP, LR | Dia 15 do 2º mês seguinte |
| EFD-ICMS/IPI | Mensal | LP, LR | Dia 15 mês seguinte |
| EFD Contribuições | Mensal | LP, LR | Dia 15 mês seguinte |
| EFD-Reinf | Mensal | LP, LR | Dia 15 mês seguinte |
| eSocial | Mensal | Todos | Dia 7 mês seguinte |
| DIRF | Anual (jan) | Todos | Último dia de fevereiro do ano seguinte |
| RAIS | Anual (jan) | Todos | 31/03 do ano seguinte |
| DEFIS | Anual (jan) | Simples | 31/03 do ano seguinte |
| SPED ECD | Anual (jan) | LP, LR | 31/05 do ano seguinte |
| SPED ECF | Anual (jan) | LP, LR | 31/07 do ano seguinte |

Obrigações anuais aparecem apenas no mês de janeiro (`mes == 1`), com vencimento no ano seguinte ao exercício.

---

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/empresas` | Lista empresas ativas |
| POST | `/api/empresas` | Cria empresa (gera obrigações automaticamente) |
| PUT | `/api/empresas/{id}` | Atualiza razão social / regime |
| DELETE | `/api/empresas/{id}` | Soft-delete |
| GET | `/api/obrigacoes/calendario` | Calendário por empresa/mês/ano (filtro de status) |
| POST | `/api/obrigacoes/gerar` | Gera obrigações para empresa/período |
| GET | `/api/obrigacoes/exportar` | Exporta CSV do calendário |
| POST | `/api/entregas/{obrigacaoId}` | Registra entrega de uma obrigação |
| GET | `/api/entregas/historico/{empresaId}` | Histórico de entregas por empresa |
| GET | `/api/dashboard` | Contagens consolidadas (mês/ano) |
| GET | `/api/dashboard/alertas` | Obrigações vencendo em 30 dias + atrasadas |
| POST | `/api/admin/seed` | Seed manual de dados de demonstração |

Documentação interativa disponível em `/swagger` após subir a API.

---

## Como rodar localmente (sem Docker)

**Pré-requisitos:** .NET 9 SDK, PostgreSQL 16 rodando localmente.

```bash
cd src/EAuditoria.API

# Ajuste a connection string em appsettings.json se necessário
dotnet run
```

A API sobe em `http://localhost:5000`. O EF Core roda as migrations e o seed automaticamente.

---

## Como rodar com Docker

Na raiz do monorepo (`e-auditoria/`):

```bash
docker compose up --build
```

| Serviço | URL |
|---------|-----|
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| Frontend | http://localhost:3000 |

---

## Testes

```bash
cd tests/EAuditoria.Tests
dotnet test
```

Cobertura unitária: `TaxRulesEngineTests`, `EmpresaServiceTests`, `ObrigacaoServiceTests`, `EntregaServiceTests`, `DashboardServiceTests`.

---

## Variáveis de ambiente (Docker)

| Variável | Valor padrão |
|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Docker` |
| `ASPNETCORE_URLS` | `http://+:8080` |
| `ConnectionStrings__Default` | `Host=postgres;Port=5432;Database=eauditoria;Username=postgres;Password=postgres` |

---

## Estrutura de pastas

```
src/
├── EAuditoria.Domain/
│   ├── Entities/           # Empresa, ObrigacaoAcessoria, EntregaObrigacao
│   ├── Enums/              # RegimeTributario, TipoObrigacao, StatusObrigacao...
│   └── Interfaces/         # IRepository<T>, IEmpresaRepository, IObrigacaoRepository...
│
├── EAuditoria.Application/
│   ├── DTOs/Request/       # CriarEmpresaRequest, RegistrarEntregaRequest...
│   ├── DTOs/Response/      # EmpresaResponse, ObrigacaoResponse, DashboardResponse...
│   ├── Engine/             # TaxRulesEngine + ITaxRulesEngine
│   ├── Helpers/            # EnumExtensions
│   ├── Interfaces/Services/ # IEmpresaService, IObrigacaoService...
│   ├── Mappings/           # DomainToDtoProfile (AutoMapper)
│   └── Services/           # EmpresaService, ObrigacaoService, EntregaService, DashboardService
│
├── EAuditoria.Infrastructure/
│   ├── Data/AppDbContext.cs
│   ├── Data/Configurations/ # Fluent API por entidade
│   ├── Data/Migrations/    # InitialCreate + ModelSnapshot
│   ├── Data/Seed/          # DatabaseSeeder
│   └── Repositories/       # BaseRepository<T> + implementações
│
└── EAuditoria.API/
    ├── Endpoints/          # Minimal API handlers por recurso (Auth, Empresas, Obrigações...)
    ├── Dependencies/       # DI separado por responsabilidade
    ├── Extensions/         # ServiceCollectionExtensions (ponto de entrada)
    ├── Middleware/         # GlobalExceptionMiddleware
    └── Program.cs          # MapGroup + endpoint registration
```
