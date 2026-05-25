---
title: OrionERP — Arquitetura & Estado Atual
type: source-of-truth
project: OrionERP
version: Abril/2026
tags: [orionerp, arquitetura, clean-architecture, csharp, multi-tenant, moc]
updated: 2026-04-09
---

← [[README|🏠 Voltar ao Projeto]]

---

**Versão:** Março/2026
**Tecnologia alvo:** [[CSharp DotNET|C# .NET 9 ]]+ EF Core + PostgreSQL
**Foco inicial:** MVP de vendas para restaurante (PDV + produção + estoque + ticket cozinha)
**Inspiração:** modelo híbrido Toast POS / iFood / Square adaptado para ERP multi-tenant

Este documento é a **fonte oficial de verdade** do OrionERP para humanos e IA. Registra o que já existe, o que está pendente, regras arquiteturais, contratos estáveis e invariantes de negócio.

---

## 🗺️ Módulos Documentados

### Arquitetura & Infraestrutura
- [[Arquitetura Geral]] — Clean Architecture, stack, estrutura de camadas
- [[Módulos/Multi-Tenancy]] — Isolamento por subdomínio, TenantProvider, Global Query Filters
- [[CQRS e MediatR]] — Commands, Queries, Handlers, pipeline
- [[ValidationBehavior]] — Pipeline de validação FluentValidation
- [[Repository Genérico]] — IRepository<T>, paginação e filtros dinâmicos 
- [[BaseCrudController]] — Controller genérico CRUD com filtros por query string
- [[ObjectMapper]] — Mapeamento por expression trees sem AutoMapper
- [[OrionDbContext]] — DbContext, migrations, auditoria automática, IDbContextFactory
- [[Processamento Assíncrono]] — Workers, filas, eventos, padrão de bootstrapping de tenant
- [[EntityBase e Auditoria]] — Classe base de entidades, campos auditáveis
- [[Exception Handling]] — Middleware centralizado de tratamento de erros
- [[Logging e Observabilidade]] — Serilog estruturado
- [[Seed de Dados]] — Dados iniciais de desenvolvimento

### Módulos de Domínio (Implementados ✅) 

- [[Autenticação JWT]] — Login, Refresh Token, Revogação
- [[Módulo Tenant]] — Raiz do sistema, escopo global
- [[Módulo Empresa]] — Unidade de negócio por tenant
- [[Módulo Usuário]] — Usuários tenant-scoped
- [[Módulo EmpresaUsuario]] — Vínculo N:N com papéis
- [[Módulo Departamento]] — Divisões organizacionais (módulo de referência)
- [[Módulo Centro de Custo]] — Agrupamento financeiro
- [[Módulo Almoxarifado]] — Locais de armazenamento
- [[Módulo Tipo de Almoxarifado]] — Catálogo global de tipos
- [[Módulo Grupo de Preparo]] — Agrupamentos de produção (restaurante)
- [[Módulo Ponto de Entrega]] — Locais de entrega de pedidos (restaurante)

- [[Módulos/Módulo Movimento Turno Caixa|Módulo Movimento / Turno / Caixa]] — Ciclo de caixa, turno de operador, lógica de carência ✅

### Módulos Parciais / Pendentes
- [[Módulo PDV]] — Entidade e configuração EF prontas; CRUD pendente ⚠️

### Especificações Futuras
- [[produto_restaurante|🍽️ Domínio de Produtos]] — Especificação pronta
- [[pedido_venda|🛒 Pedidos e Vendas]] — Especificação pronta
- [[nota_fiscal|🧾 Notas Fiscais]] — Especificação pronta (shells Phase 1 + emissão Fase 2)
- [[comanda|🪑 Comanda / Mesa]] — Especificação pronta (Fase 2)
- [[Cartões Internos|Cartões Internos]] ← especificação em construção
- Estoque --> necessário análise e estudo de caso
- Integrações Hits --> necessário análise e estudo de caso

### Testes
- [[Testes]] — Estratégia, cobertura atual e pendências

---

## 🎯 Visão Geral Rápida

| Aspecto | Status |
|---|---|
| Arquitetura | Clean Architecture - 4 Camadas ✅ |
| Banco de Dados | EF Core + PostgreSQL ✅ |
| Autenticação | JWT + Refresh Token ✅ |
| Logging | Serilog (console + arquivo) ✅ |
| Multi-tenancy | Host-based + Slug Strategy ✅ |
| Exception Handling | Middleware Centralizado ✅ |
| API Documentation | Swagger/OpenAPI ✅ |
| Repositórios | IRepository\<T\> genérico ✅ |
| CQRS / MediatR | Commands + Queries implementados ✅ |
| Endpoints de Negócio | Auth, Empresa, Usuário, Departamento, CentroCusto, Almoxarifado, TipoAlmoxarifado, GrupoPreparo, PontoEntrega, Movimento, Turno, EmpresaParametros ✅ |
| Projeto de Testes | Implementado ✅ |
| Compilação | Sem erros ✅ |

---

## 📁 Estrutura do Projeto

```
/src
├── /OrionERP.API           → Controllers, Middlewares, Program.cs
├── /OrionERP.Application   → CQRS, Handlers, Validators, DTOs, Interfaces
├── /OrionERP.Domain        → Entities, EntityBase, ITenantAware
├── /OrionERP.Infrastructure → EF Core, Repository, Services, Seeds
└── /OrionERP.Tests         → Unit + Integration
```

---

## 🔒 Escopo Multi-Tenant por Entidade

| Entidade | TenantScoped | EmpresaScoped | Global |
|---|---|---|---|
| Tenant | ❌ | ❌ | ✅ |
| Empresa | ✅ | — | ❌ |
| Usuario | ✅ | ❌ | ❌ |
| EmpresaUsuario | ✅ | ✅ | ❌ |
| Departamento | ❌ | ✅ | ❌ |
| CentroCusto | ❌ | ✅ | ❌ |
| Almoxarifado | ❌ | ✅ | ❌ |
| TipoAlmoxarifado | ❌ | ❌ | ✅ |
| GrupoPreparo | ❌ | ✅ | ❌ |
| PontoEntrega | ❌ | ✅ | ❌ |
| Pdv | ❌ | ✅ | ❌ |
| EmpresaParametros | ❌ | ✅ | ❌ |
| Movimento | ❌ | ✅ | ❌ |
| Turno | ❌ | ✅ | ❌ |
| AuditoriaMovimento | ❌ | ✅ | ❌ |
| RefreshToken | ❌ | ❌ | ✅ |

---

## ⚖️ Invariantes de Negócio

- Empresa sempre pertence a um Tenant
- Departamento sempre pertence a uma Empresa
- CentroCusto sempre pertence a uma Empresa
- Almoxarifado deve possuir TipoAlmoxarifado válido
- Pdv deve pertencer a uma Empresa
- Usuario pode existir sem vínculo imediato com Empresa
- Multi-tenancy nunca pode ser burlado manualmente
- Existe no máximo 1 Movimento Aberto por empresa (unique index condicional)
- Não é possível fechar Movimento com Turnos em aberto
- Existe no máximo 1 Turno Aberto por PDV + Operador (unique index condicional)
- Fechamento de Movimento gera AuditoriaMovimento automaticamente via notification

---

## 🔐 Contratos Estáveis (Não Quebrar Sem Justificativa)

- `BaseCrudController<>`
- `IRepository<T>`
- `ObjectMapper`
- `TenantProvider` + `ITenantProvider` (incluindo `HasContext`)
- `Global Query Filters` (padrão closure, não snapshot)
- `IDbContextFactory<OrionDbContext>` (para workers)
- Padrão de bootstrapping de tenant em workers (`SetCurrentTenantContext` obrigatório)

> Mudanças nesses componentes exigem justificativa arquitetural explícita.

---

## 🔍 Auditoria Técnica

Auditoria realizada em 2026-03-27. Correções aplicadas em 2026-03-28. Ver relatório completo: [[auditoria-tecnica-2026-03]]

**Todos os desvios críticos corrigidos** ✅

---

## ⚠️ Riscos Conhecidos

- Filtros dinâmicos podem gerar queries complexas
- Uso excessivo de `AsQueryable()` pode afetar performance
- Multi-tenancy depende fortemente do `TenantMiddleware` em contexto HTTP
- Em workers, **ausência de `SetCurrentTenantContext`** bypassa filtros de leitura — leituras cross-tenant possíveis sem o guard
- Queries críticas devem ser revisadas via SQL gerado

---

## 🚀 Próximos Passos

### Fase 1 – CRUDs Restantes
- [x] Auth, Empresa, Usuário, Departamento ✅
- [x] CentroCusto ✅
- [x] Almoxarifado ✅
- [x] TipoAlmoxarifado ✅
- [x] GrupoPreparo ✅
- [x] PontoEntrega ✅
- [ ] PDV — CRUD pendente (entidade e configuração EF prontas)

### Fase 2 – Módulos de Negócio
- [ ] [[movimento_turno_caixa|💰 Movimento / Turno / Caixa]] ← especificação pronta
- [ ] [[produto_restaurante|🍽️ Produtos & Cardápio]] ← especificação pronta
- [ ] [[pedido_venda|🛒 Pedidos e Vendas]] ← especificação pronta
- [ ] [[nota_fiscal|🧾 Notas Fiscais]] — shells Phase 1 + emissão Fase 2 ← especificação pronta
- [ ] [[comanda|🪑 Comanda / Mesa]] ← especificação pronta (Fase 2)
- [ ] [[Cartões Internos|Cartões Internos]] ← especificação em construção
- [ ] [[estoque_nfe_spec|Estoque]] ← especificação em construção

### Fase 3 – Evoluções Estruturais
- [ ] Soft Delete
- [ ] Auditoria (CreatedBy/UpdatedBy)
- [ ] Permissões avançadas via EmpresaUsuario

### Fase 4 – Expansão de Testes
- [ ] Cobertura multi-tenant completa
- [ ] Testes de integração com banco real para todos os módulos

### Fase 5 – Segurança & Performance
- [ ] Rate Limiting
- [ ] Caching
- [ ] Índices otimizados

---

## 🧭 Como Adicionar Nova Feature

Seguir o padrão de [[Módulo Departamento]]:

1. **Domain** → entidade + `ITenantAware` se necessário
2. **Infrastructure** → `IEntityTypeConfiguration<T>` + DbSet no contexto + Global Query Filter se EmpresaScoped
3. **Application** → Commands, Queries, Models, Validators (dentro de `Features/NomeDaFeature/`)
4. **API** → Controller herdando `BaseCrudController<>`

Sempre validar: DRY · KISS · YAGNI · Segurança multi-tenant

---

## 📘 Glossário de Domínio

| Termo | Descrição |
|---|---|
| **Tenant** | Organização/cliente principal. Isolamento lógico dos dados. |
| **Empresa** | Unidade de negócio dentro de um tenant. Tem slug próprio. |
| **Usuario** | Usuário pertencente a um tenant, com acesso a uma ou mais empresas. |
| **EmpresaUsuario** | Relação usuário-empresa com papel e permissões. |
| **Departamento** | Divisão organizacional dentro de uma empresa. |
| **CentroCusto** | Centro de custo para agrupamento financeiro dentro de uma empresa. |
| **Almoxarifado** | Local de armazenamento de materiais. |
| **TipoAlmoxarifado** | Categoria/tipo de almoxarifado (catálogo global). |
| **GrupoPreparo** | Agrupamento de itens por estação de produção (restaurante). |
| **PontoEntrega** | Local de entrega de pedidos (mesa, balcão, etc.). |
| **Pdv** | Ponto de venda vinculado a uma empresa. |
| **Slug** | Identificador legível usado no host (ex: `empresaA.orionerp.com`). |

---

## ⚙️ Comandos Úteis

Todos executados a partir de `src/`:

```bash
# Build da solução
dotnet build OrionERP.sln

# Executar a API
dotnet run --project OrionERP.API/OrionERP.API.csproj

# Adicionar migration (executar de OrionERP.API/)
dotnet ef migrations add <NomeDaMigration> --project ../OrionERP.Infrastructure --startup-project .

# Aplicar migrations
dotnet ef database update --project ../OrionERP.Infrastructure --startup-project .

# Testes
dotnet test OrionERP.Tests/OrionERP.Tests.csproj
```

> A API escuta em `http://0.0.0.0:5000`. Swagger disponível em `/swagger`.
