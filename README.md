# e-Auditoria — Painel de Obrigações Acessórias

Sistema SaaS de gestão tributária para escritórios contábeis. Permite controlar obrigações fiscais de múltiplos CNPJs em regimes tributários distintos, eliminando dependência de planilhas.

---

## Índice

1. [O que foi construído](#o-que-foi-construído)
2. [Arquitetura geral](#arquitetura-geral)
3. [Estrutura do monorepo](#estrutura-do-monorepo)
4. [Decisões técnicas](#decisões-técnicas)
5. [Backend — Clean Architecture](#backend--clean-architecture)
6. [Frontend — estrutura e padrões](#frontend--estrutura-e-padrões)
7. [Engine de regras tributárias](#engine-de-regras-tributárias)
8. [Banco de dados](#banco-de-dados)
9. [Autenticação e segurança](#autenticação-e-segurança)
10. [Docker e infraestrutura](#docker-e-infraestrutura)
11. [Como subir o projeto](#como-subir-o-projeto)
12. [Desenvolvimento local sem Docker](#desenvolvimento-local-sem-docker)
13. [Testes](#testes)
14. [Diferenciais implementados](#diferenciais-implementados)
15. [Uso de IA — BMAD Method](#uso-de-ia--bmad-method)
16. [.gitignore recomendado](#gitignore-recomendado)

---

## O que foi construído

| Funcionalidade | Descrição |
|---|---|
| Gestão de empresas | Cadastro, listagem e remoção com CNPJ validado e regime tributário |
| Engine de obrigações | Cálculo automático de quais obrigações cabem a cada regime e seus vencimentos |
| Calendário mensal | Visualização por empresa e competência com filtro de status |
| Registro de entrega | Marca obrigação como entregue com data de conclusão e observação |
| Histórico de entregas | Linha do tempo por empresa |
| Painel de alertas | Vencendo em 30 dias e atrasadas, ordenadas por urgência |
| Dashboard | KPIs consolidados, gráficos de distribuição e alertas por janela de tempo |
| Exportação CSV | Calendário exportável em UTF-8 BOM (compatível com Excel) |
| Autenticação JWT | Login com token Bearer, proteção de rotas no front e no back |
| Seed automático | 10 empresas + ~500 obrigações de demonstração na primeira inicialização |
| Geração automática | Ao cadastrar empresa, obrigações de 12 meses futuros são geradas imediatamente |

---

## Arquitetura geral

```
┌─────────────────────────────────────────────────────┐
│                    Browser (React)                   │
│   TanStack Query → Axios → /api/* (Nginx proxy)     │
└─────────────────────────┬───────────────────────────┘
                          │ HTTP (porta 3000 → proxy)
┌─────────────────────────▼───────────────────────────┐
│              Nginx (Alpine, porta 80)                │
│  /api/* → proxy_pass http://api:8080                 │
│  /*     → serve /usr/share/nginx/html (SPA)          │
└─────────────────────────┬───────────────────────────┘
                          │ HTTP interno (porta 8080)
┌─────────────────────────▼───────────────────────────┐
│           ASP.NET Core 9 (.NET 9)                    │
│  Controllers → Services → Repositories → EF Core    │
└─────────────────────────┬───────────────────────────┘
                          │ TCP (porta 5432)
┌─────────────────────────▼───────────────────────────┐
│              PostgreSQL 16                           │
└─────────────────────────────────────────────────────┘
```

Toda comunicação frontend → backend passa pelo Nginx. O frontend nunca faz chamada direta à porta do backend — isso garante que o mesmo `baseURL: '/api'` funcione tanto em Docker quanto localmente via proxy do Vite.

---

## Estrutura do monorepo

```
e-auditoria/
├── docker-compose.yml
├── README.md                          ← este arquivo
│
├── e-Auditoria-Api/                   ← backend .NET 9
│   ├── src/
│   │   ├── EAuditoria.Domain/         ← entidades, enums, interfaces de repositório
│   │   ├── EAuditoria.Application/    ← DTOs, serviços, engine, interfaces de serviço
│   │   ├── EAuditoria.Infrastructure/ ← EF Core, repositórios, seed, migrations
│   │   └── EAuditoria.API/            ← controllers, DI, middleware, Dockerfile
│   ├── tests/
│   │   └── EAuditoria.Tests/          ← xUnit: engine + serviços
│   └── README.md
│
└── e-Auditoria-Front/                 ← frontend React 18 + Vite 5
    ├── src/
    │   ├── components/                ← componentes reutilizáveis + layout
    │   ├── contexts/                  ← ThemeContext (tema + displayName)
    │   ├── pages/                     ← Dashboard, Empresas, Calendário, Alertas, Login
    │   ├── services/                  ← camada HTTP (Axios, um arquivo por recurso)
    │   ├── styles/                    ← global.css
    │   └── types/                     ← contratos TypeScript espelhando os DTOs da API
    ├── Dockerfile
    ├── nginx.conf
    └── README.md
```

---

## Decisões técnicas

### Por que Clean Architecture e não Minimal APIs puras?

O escopo do case exige separação clara entre regras de negócio (cálculo de obrigações e vencimentos) e infraestrutura (banco, HTTP). Colocar essa lógica em handlers de Minimal API dificultaria o teste unitário da engine tributária sem subir banco ou contexto HTTP. A Clean Architecture isola a `TaxRulesEngine` no Application layer, tornando-a testável com `new TaxRulesEngine()`.

### Por que 4 projetos e não 1?

| Projeto | Responsabilidade única |
|---|---|
| `Domain` | Entidades, enums, interfaces de repositório — zero dependências externas |
| `Application` | Regras de negócio, DTOs, AutoMapper, serviços — depende só do Domain |
| `Infrastructure` | EF Core, Npgsql, repositórios concretos, seed — depende de Application |
| `API` | Controllers, DI, middleware, Swagger — orquestra os três anteriores |

Isso garante que `Domain` e `Application` nunca conheçam EF Core ou ASP.NET — a regra de negócio é agnóstica de framework.

### Por que `IEmpresaRepository` fica no Domain e `IUsuarioRepository` no Application?

`IEmpresaRepository`, `IObrigacaoRepository` e `IEntregaRepository` fazem parte da linguagem ubíqua do domínio tributário — são contratos de negócio. `IUsuarioRepository` é um detalhe de infraestrutura de autenticação, sem relevância para as regras fiscais, portanto vive no Application layer junto com o `IAuthService`.

### Por que AutoMapper e não mapeamento manual?

Com 6+ entidades e DTOs com nomes distintos (snake_case no banco, PascalCase no C#, camelCase no JSON), o AutoMapper elimina boilerplate repetitivo e centraliza as regras de projeção em `DomainToDtoProfile`. O custo de configuração é pago uma vez; o ganho é legibilidade em todos os pontos de uso.

### Por que Serilog?

O `ILogger` nativo do ASP.NET não possui structured logging adequado para produção. Serilog permite adicionar sinks (console, arquivo, Seq, Datadog) sem alterar código de negócio. O `appsettings.json` controla os níveis por namespace, separando o ruído do EF Core da lógica de aplicação.

### Por que TanStack Query e não SWR ou fetch puro?

TanStack Query (React Query v5) é o estado da arte para estado de servidor em React. Oferece cache automático, refetch por foco/reconexão, invalidação por chave de query, e mutations com rollback — tudo com tipagem forte. O `staleTime: 30_000` evita refetches desnecessários sem prejudicar a consistência.

### Por que Zustand não foi necessário?

Todo estado relevante é estado de servidor (empresas, obrigações, alertas), gerenciado pelo TanStack Query. O único estado de UI compartilhado (tema + displayName) é gerenciado pelo `ThemeContext` próprio, simples o suficiente para não justificar uma dependência extra.

---

## Backend — Clean Architecture

### Fluxo de uma requisição

```
HTTP Request
  → GlobalExceptionMiddleware (captura exceções não tratadas)
  → JWT Bearer Middleware (valida token)
  → Controller (valida model binding, chama Service)
  → Service (lógica de aplicação, AutoMapper)
  → Repository (LINQ + EF Core)
  → AppDbContext
  → PostgreSQL
  → Response (DTO serializado como JSON com enums como string)
```

### Convenções de DI

| Lifetime | Usado para |
|---|---|
| `Singleton` | `ITaxRulesEngine` — stateless, compartilhado entre requisições |
| `Scoped` | Serviços, repositórios, `AppDbContext`, `DatabaseSeeder` |
| `Transient` | Não utilizado — nenhum componente tem estado por-resolve |

### Organização dos Dependencies

A pasta `src/EAuditoria.API/Dependencies/` separa o registro de DI por responsabilidade:

| Arquivo | O que registra |
|---|---|
| `InfrastructureDependencies.cs` | `AppDbContext`, `DatabaseSeeder` |
| `RepositoryDependencies.cs` | Todos os repositórios |
| `ServiceDependencies.cs` | Engine, serviços de aplicação |
| `AuthDependencies.cs` | JWT Bearer, `IAuthService` |
| `SwaggerDependencies.cs` | OpenAPI com segurança JWT |
| `CorsDependencies.cs` | Política CORS para o frontend |

O `Program.cs` apenas orquestra — sem lógica de registro inline.

### Tratamento de erros

O `GlobalExceptionMiddleware` intercepta todas as exceções não tratadas e retorna `ProblemDetails` (RFC 7807) com status adequado. `UnauthorizedAccessException` → 401. Exceções genéricas → 500. Controllers retornam `ActionResult<T>` tipado, sem `try/catch` repetido.

---

## Frontend — estrutura e padrões

### Camada de serviços

Cada recurso tem seu arquivo em `src/services/`:

```
services/
├── api.ts          ← instância Axios base + interceptors (token + 401)
├── auth.ts         ← login, logout, getToken, getUser, isAuthenticated
├── empresas.ts     ← CRUD de empresas + geração de obrigações
├── obrigacoes.ts   ← calendário, filtros, registro de entrega
├── dashboard.ts    ← métricas consolidadas + alertas
└── admin.ts        ← seed de demonstração
```

O `api.ts` injeta `Authorization: Bearer <token>` em toda requisição via interceptor de request. O interceptor de response redireciona para `/login` em qualquer 401, sem precisar tratar isso em cada hook.

### Proteção de rotas

`ProtectedRoute` lê `authService.isAuthenticated()` (presença do token em localStorage). Rotas autenticadas ficam aninhadas sob esse componente no `App.tsx`. A lógica de redirect está em um único lugar.

### ThemeContext

`src/contexts/ThemeContext.tsx` persiste em `localStorage` três valores: `activeThemeId`, `primaryColor` e `displayName`. O `ThemedApp` em `main.tsx` consome o context e repassa `colorPrimary` ao `ConfigProvider` do Ant Design — toda a interface reage à mudança sem reload.

### Contratos TypeScript

`src/types/index.ts` espelha os DTOs da API. Qualquer mudança de contrato no backend é detectada em compile time no frontend — sem `any`, sem casting implícito.

---

## Engine de regras tributárias

A `TaxRulesEngine` (`src/EAuditoria.Application/Engine/`) encapsula toda a lógica de qual obrigação se aplica a qual regime e como calcular o vencimento.

### Matriz de obrigações

| Obrigação | Periodicidade | Simples | Presumido | Real | Imunidade |
|---|---|---|---|---|---|
| DAS | Mensal | ✓ | — | — | — |
| DEFIS | Anual (jan) | ✓ | — | — | — |
| DCTF | Mensal | — | ✓ | ✓ | — |
| EFD-ICMS/IPI | Mensal | — | ✓ | ✓ | — |
| EFD Contribuições | Mensal | — | ✓ | ✓ | — |
| EFD-Reinf | Mensal | — | ✓ | ✓ | — |
| SPED ECD | Anual (jan) | — | ✓ | ✓ | — |
| SPED ECF | Anual (jan) | — | ✓ | ✓ | — |
| eSocial | Mensal | ✓ | ✓ | ✓ | — |
| DIRF | Anual (jan) | ✓ | ✓ | ✓ | — |
| RAIS | Anual (jan) | ✓ | ✓ | ✓ | — |

Obrigações anuais aparecem **apenas em janeiro** de cada ano, com vencimento no calendário fiscal correto.

### Regras de vencimento

| Obrigação | Prazo |
|---|---|
| DAS | Dia 20 do mês seguinte; se fim de semana, próximo dia útil |
| DCTF | Dia 15 do segundo mês seguinte |
| EFD-ICMS/IPI | Dia 15 do mês seguinte |
| EFD Contribuições | Dia 15 do mês seguinte |
| eSocial | Dia 7 do mês seguinte |
| EFD-Reinf | Dia 15 do mês seguinte |
| SPED ECD | 31 de maio do ano seguinte |
| SPED ECF | 31 de julho do ano seguinte |
| DIRF | Último dia de fevereiro do ano seguinte |
| RAIS / DEFIS | 31 de março do ano seguinte |

Todos os `DateTime` são criados com `DateTimeKind.Utc` explícito — requisito do driver Npgsql para colunas `timestamp with time zone`.

---

## Banco de dados

### Schema

```sql
empresas
  id uuid PK
  razao_social varchar(200)
  cnpj varchar(14) UNIQUE
  regime_tributario int (enum)
  ativo bool DEFAULT true
  criado_em timestamptz
  atualizado_em timestamptz

obrigacoes_acessorias
  id uuid PK
  empresa_id uuid FK → empresas
  tipo int (enum)
  periodicidade int (enum)
  competencia int (mês 1-12)
  ano_competencia int
  vencimento timestamptz
  status int (enum)
  criado_em timestamptz
  UNIQUE (empresa_id, tipo, competencia, ano_competencia)

entregas_obrigacoes
  id uuid PK
  obrigacao_id uuid FK → obrigacoes_acessorias UNIQUE
  data_entrega timestamptz
  observacao varchar(500)
  criado_em timestamptz

usuarios
  id uuid PK
  nome varchar(200)
  email varchar(200) UNIQUE
  senha_hash varchar(500)
  perfil varchar(50)
  ativo bool DEFAULT true
  criado_em timestamptz
  ultimo_login_em timestamptz
```

### Índices

| Índice | Justificativa |
|---|---|
| `ix_empresas_cnpj` (UNIQUE) | Busca por CNPJ na importação/validação |
| `ix_obrigacoes_empresa_tipo_competencia` (UNIQUE) | Garante idempotência do seed e geração automática |
| `ix_obrigacoes_empresa_mes_ano` | Filtragem do calendário por empresa + período |
| `ix_obrigacoes_vencimento_status` | Queries do painel de alertas (ORDER BY vencimento) |
| `ix_entregas_obrigacao_id` (UNIQUE) | 1-para-1 entre obrigação e entrega |
| `ix_usuarios_email` (UNIQUE) | Lookup de login por e-mail |

### Migrations

As migrations são versionadas em `src/EAuditoria.Infrastructure/Data/Migrations/`. O arquivo `20250101000000_InitialCreate.cs` contém o schema completo com os atributos `[DbContext]` e `[Migration]` necessários para que o EF Core localize a migration em runtime no container Docker (sem CLI tools).

Na inicialização, o `Program.cs` verifica `GetPendingMigrationsAsync()` antes de chamar `MigrateAsync()`. Se nenhuma migration estiver registrada no assembly (situação de fallback), chama `EnsureCreatedAsync()` para garantir que o schema exista.

---

## Autenticação e segurança

### Fluxo

```
POST /api/auth/login { email, senha }
  → AuthService.LoginAsync()
  → BCrypt.Verify(senha, usuario.SenhaHash)
  → JwtSecurityTokenHandler.WriteToken()
  → { token, expiresInSeconds: 28800, nome, email, perfil }

Todas as demais rotas:
  → Authorization: Bearer <token>
  → JwtBearerMiddleware valida assinatura + issuer + audience + expiração
  → ClaimsPrincipal disponível no Controller
```

### Configuração JWT

```json
"Jwt": {
  "Key": "e-Auditoria-Secret-Key-Minimum-32-Characters-Long!",
  "Issuer": "e-auditoria",
  "Audience": "e-auditoria-frontend"
}
```

> Em produção, `Jwt:Key` deve vir de variável de ambiente ou secret manager — nunca em código-fonte.

### Usuário padrão de demonstração

| Campo | Valor |
|---|---|
| E-mail | `admin@eauditoria.com.br` |
| Senha | `Admin@2025` |
| Perfil | `Admin` |

Criado automaticamente pelo `DatabaseSeeder` na primeira inicialização. Hash BCrypt com work factor padrão (11 rounds).

### Proteção por perfil

`AdminController` requer `[Authorize(Roles = "Admin")]`. Os demais controllers requerem apenas `[Authorize]` — qualquer usuário autenticado tem acesso.

---

## Docker e infraestrutura

### Serviços

| Serviço | Imagem base | Porta interna | Porta exposta |
|---|---|---|---|
| `postgres` | `postgres:16-alpine` | 5432 | 5432 |
| `api` | Multi-stage .NET 9 SDK → ASP.NET runtime | 8080 | 8080 |
| `frontend` | Multi-stage Node 20 → Nginx Alpine | 80 | 3000 |

### Build da API — multi-stage

```dockerfile
# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/EAuditoria.API/EAuditoria.API.csproj -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EAuditoria.API.dll"]
```

O stage de build não entra na imagem final. A imagem de runtime tem ~220 MB.

### Build do frontend — multi-stage

```dockerfile
# Stage 1: build
FROM node:20-alpine AS build
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm install
COPY . .
RUN npm run build

# Stage 2: serve
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
```

### nginx.conf — decisões importantes

```nginx
location /api/ {
    proxy_pass http://api:8080/api/;
}

location / {
    try_files $uri $uri/ /index.html;
}
```

O `try_files` com fallback para `index.html` é obrigatório para SPAs com React Router — sem isso, qualquer reload em rota diferente de `/` retorna 404.

### Healthcheck e ordem de inicialização

```yaml
postgres:
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 5s
    retries: 10

api:
  depends_on:
    postgres:
      condition: service_healthy
```

A API só sobe depois que o PostgreSQL aceitar conexões. Sem isso, o `MigrateAsync()` falharia na primeira inicialização.

### `docker-compose.yml` da raiz

```yaml
version: "3.9"

services:
  postgres:
    image: postgres:16-alpine
    container_name: eauditoria_postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: eauditoria
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 10

  api:
    build:
      context: ./e-Auditoria-Api
      dockerfile: src/EAuditoria.API/Dockerfile
    container_name: eauditoria_api
    environment:
      ASPNETCORE_ENVIRONMENT: Docker
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__Default: "Host=postgres;Port=5432;Database=eauditoria;Username=postgres;Password=postgres"
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy
    restart: on-failure

  frontend:
    build:
      context: ./e-Auditoria-Front
      dockerfile: Dockerfile
    container_name: eauditoria_frontend
    ports:
      - "3000:80"
    depends_on:
      - api
    restart: on-failure

volumes:
  postgres_data:
```

> Este `docker-compose.yml` deve ficar na **raiz do monorepo** (`e-auditoria/`), não dentro de nenhuma subpasta.

---

## Como subir o projeto

```bash
git clone <repo-url> e-auditoria
cd e-auditoria
docker compose up --build
```

| URL | Descrição |
|---|---|
| http://localhost:3000 | Frontend |
| http://localhost:8080/swagger | API — Swagger UI com autenticação JWT |
| http://localhost:5432 | PostgreSQL (usuário: `postgres`, senha: `postgres`) |

Na primeira inicialização o seed é executado automaticamente. Nenhum passo manual é necessário.

Para derrubar e limpar o volume do banco:

```bash
docker compose down -v
```

---

## Desenvolvimento local sem Docker

### Backend

```bash
cd e-Auditoria-Api

# Requer PostgreSQL local na porta 5432
# Ajuste a connection string em src/EAuditoria.API/appsettings.json se necessário

dotnet restore
dotnet run --project src/EAuditoria.API
# API disponível em http://localhost:5226
```

### Frontend

```bash
cd e-Auditoria-Front
npm install
npm run dev
# Frontend em http://localhost:5173
# Proxy /api → http://localhost:5226 (configurado em vite.config.ts)
```

### Variáveis relevantes

| Variável | Padrão local | Padrão Docker |
|---|---|---|
| `ConnectionStrings__Default` | `Host=localhost;...` | `Host=postgres;...` |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Docker` |
| `Jwt__Key` | Definida em `appsettings.json` | Mesma chave (via env ou arquivo) |

---

## Testes

```bash
cd e-Auditoria-Api
dotnet test tests/EAuditoria.Tests/
```

| Classe de teste | O que cobre |
|---|---|
| `TaxRulesEngineTests` | Engine por regime × obrigação × vencimento (~20 cenários) |
| `EmpresaServiceTests` | CRUD, validação de CNPJ duplicado |
| `ObrigacaoServiceTests` | Filtros, cálculo de status por data |
| `EntregaServiceTests` | Registro de entrega, idempotência |
| `DashboardServiceTests` | Contagens consolidadas, alertas por janela |

A engine tributária é coberta prioritariamente — é o núcleo do negócio e o componente mais propenso a regressão em mudanças de prazo ou regime.

---

## Diferenciais implementados

- Geração automática de obrigações (12 meses futuros + anos vigentes) ao cadastrar empresa
- Filtro de status no calendário
- Histórico de entregas com Timeline (Ant Design)
- Exportação em CSV (UTF-8 BOM, compatível com Excel)
- Botão "Ver calendário" nas Empresas com navegação direta ao período
- Registro de entrega diretamente do Painel de Alertas
- Dashboard com banner inteligente detectando banco sem dados
- Endpoint `POST /api/admin/seed` para repovoar dados sem reiniciar containers
- Autenticação JWT com BCrypt + perfis de acesso
- Personalização de tema e nome de exibição persistidos por usuário (localStorage)

---

## Uso de IA — BMAD Method

Este projeto foi desenvolvido com **Claude (Anthropic)** como agente de desenvolvimento, seguindo o padrão **BMAD (Business, Model, Architecture, Development)**.

### O que é BMAD

BMAD é uma abordagem onde o desenvolvedor atua como **orquestrador** e a IA como **agente executor especializado**, trabalhando em camadas progressivas:

1. **Business** — O problema de negócio é definido pelo desenvolvedor
2. **Model** — A IA analisa os requisitos e propõe o modelo de dados e regras de negócio
3. **Architecture** — A IA projeta a arquitetura (camadas, contratos de interface, estrutura de pastas)
4. **Development** — A IA executa a implementação camada por camada, guiada e corrigida pelo desenvolvedor

### Aplicação neste projeto

**Fase de arquitetura**
O desenvolvedor descreveu o problema de negócio e as restrições técnicas do edital. A IA propôs a estrutura completa: 4 projetos .NET separados por responsabilidade, `TaxRulesEngine` como Singleton stateless, seed em duas fases independentes, e a hierarquia de dependências Domain → Application → Infrastructure → API.

**Ciclos de correção**

| Problema | Causa | Correção |
|---|---|---|
| DTOs com construtor posicional | IA usou `record` com argumentos | Convertido para `class` com `get; set;` |
| `MigrateAsync()` não localizava migrations | `MigrationsAssembly` ausente | Adicionado ao `UseNpgsql()` |
| `DateTime Kind=Unspecified` no PostgreSQL | `.Date` retorna `Kind=Unspecified` | Substituído por `new DateTime(..., DateTimeKind.Utc)` |
| Seed pulando obrigações em restart | Verificação de fase única | Refatorado para duas fases independentes |
| Ambiguidade em `AddInfrastructure` | Dois métodos com mesmo nome | Renomeado para `AddInfrastructureServices` |

**Onde a IA contribuiu mais**
- Suite de testes com ~40 cenários cobrindo edge cases de regime × obrigação × vencimento
- Docker Compose com healthcheck, depends_on e proxy Nginx
- Frontend: 4 páginas, navegação com estado, Timeline, modais, exportação CSV, autenticação JWT

**Onde o desenvolvedor teve que guiar**
- Convenção de DTO do projeto (não é um padrão universal)
- `DateTime Kind=Unspecified` como bug latente — não aparece no build, só em runtime com PostgreSQL
- Identificação de que o seed pulava obrigações em reinicializações
- Decisão de estrutura monorepo vs. repositórios separados
- Estratégia de README e documentação técnica para avaliação

### Conclusão

O modelo correto para BMAD em projetos reais: **a IA acelera a execução em 5–10x; o desenvolvedor garante qualidade, coerência arquitetural e identifica os edge cases que a IA não vê sem feedback de runtime.** O resultado depende diretamente do nível técnico do orquestrador.