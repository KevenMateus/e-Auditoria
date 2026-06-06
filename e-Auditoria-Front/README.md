# e-Auditoria Front — Frontend

Interface web do Painel de Obrigações Acessórias da e-Auditoria.  
Construída com **React 18 + Vite + TypeScript + Ant Design**.

---

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Framework | React 18 + Vite 5 |
| Linguagem | TypeScript 5 |
| Componentes UI | Ant Design 5 |
| Estado servidor | TanStack Query (React Query v5) |
| Estado de UI | Zustand (tema/configurações globais) |
| Roteamento | React Router v6 |
| HTTP | Axios |
| Datas | Day.js (peer do Ant Design) |
| Container | Docker + Nginx Alpine |

---

## Páginas

| Rota | Página | Descrição |
|------|--------|-----------|
| `/` | Dashboard | Visão consolidada: totais, taxas de entrega e atraso |
| `/empresas` | Empresas | Listagem, cadastro e remoção de empresas |
| `/calendario` | Calendário | Calendário de obrigações por empresa/mês + histórico de entregas |
| `/alertas` | Alertas | Obrigações vencendo em 30 dias e já atrasadas |

---

## Arquitetura frontend

```
src/
├── components/       # Componentes reutilizáveis (ex: StatusBadge)
├── pages/            # Uma página por rota
│   ├── DashboardPage.tsx
│   ├── EmpresasPage.tsx
│   ├── CalendarioPage.tsx   # Tabs: Calendário + Histórico de Entregas
│   └── AlertasPage.tsx
├── services/         # Camada HTTP (axios) por domínio
│   ├── api.ts        # Instância axios com baseURL e interceptors
│   ├── empresas.ts
│   ├── obrigacoes.ts
│   ├── entregas.ts
│   ├── dashboard.ts
│   └── admin.ts
├── types/            # Interfaces TypeScript espelhando os DTOs da API
├── App.tsx           # Layout principal com Ant Design Layout + Menu lateral
└── main.tsx          # Bootstrap: QueryClientProvider + RouterProvider
```

### Decisões técnicas

**TanStack Query para todo estado servidor**  
Cada chamada de API é um `useQuery` com `queryKey` bem definido. As mutações usam `useMutation` + `queryClient.invalidateQueries` para manter a UI consistente sem re-fetch manual. Sem useState/useEffect para dados remotos.

**Axios com interceptor de erro global**  
O interceptor no `api.ts` extrai `error.response?.data?.mensagem` e repassa como `Error.message` padronizado. Os componentes só precisam tratar o caso de erro sem conhecer o shape do HTTP error.

**Navegação com estado via React Router**  
O botão "Ver calendário" em Empresas usa `navigate('/calendario', { state: { empresaId } })` — evita query string e mantém o estado temporário sem poluir a URL.

**Proxy Nginx em produção, Vite proxy em dev**  
Em desenvolvimento, Vite proxy `/api` para `http://localhost:5000`.  
Em produção (Docker), Nginx faz proxy reverso de `/api/` para `http://api:8080/api/` — o frontend nunca conhece o host do backend.

---

## Como rodar localmente

**Pré-requisitos:** Node 20+, backend rodando em `http://localhost:5000`.

```bash
npm install
npm run dev
```

Acesse `http://localhost:3000`.

---

## Como rodar com Docker

Na raiz do monorepo (`e-auditoria/`):

```bash
docker compose up --build
```

O frontend fica em `http://localhost:3000`.

---

## Variáveis de ambiente

O frontend não usa variáveis de ambiente em runtime — a URL da API é configurada via proxy (Vite em dev, Nginx em prod).

---

## Build de produção

```bash
npm run build   # gera dist/
npm run preview # serve o build localmente para verificação
```

O `npm run build` executa `tsc && vite build` — TypeScript é verificado antes do bundle.
