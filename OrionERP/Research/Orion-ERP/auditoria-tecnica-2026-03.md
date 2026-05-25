---
title: Auditoria Técnica — OrionERP
type: audit
project: OrionERP
date: 2026-03-27
status: corrigido
corrigido-em: 2026-03-28
tags: [auditoria, technical-debt, multi-tenancy, efcore, csharp]
---

← [[Orion-ERP|🏠 Voltar ao Projeto]]

---

# Auditoria Técnica — Março 2026

Auditoria realizada em 2026-03-27 avaliando conformidade com os padrões definidos em [[implementation-rules]], [[architecture]], [[Multi-Tenancy]] e [[patterns]].

---

## Status Geral — Pós-Correção (2026-03-28)

| Critério | Status |
|---|---|
| Clean Architecture | ✅ Conforme |
| CQRS / MediatR | ✅ Corrigido |
| Multi-tenancy | ✅ Corrigido |
| EF Core | ✅ Corrigido |
| C# Moderno (records, required, init) | ⚠️ Parcial (M7 pendente) |
| ObjectMapper (sem AutoMapper) | ✅ Conforme |
| FluentValidation | ✅ Corrigido |
| Feature-Based Organization | ✅ Conforme |
| Controllers | ✅ Conforme |
| Nomenclatura | ✅ Corrigido |

---

## 🔴 Desvios Críticos — ✅ Todos Corrigidos em 2026-03-28

### C1 — Global Query Filters por valor (vazamento entre tenants) ✅

**Arquivo:** `OrionDbContext.cs:49-63`
**Impacto:** Multi-tenant quebrado

Os global query filters capturam `tenantId` e `empresaId` por valor no momento da criação do DbContext:

```csharp
// ERRADO — captura snapshot no primeiro DbContext instanciado
var tenantId = _tenantProvider.TenantId;
var empresaId = _tenantProvider.EmpresaId;
modelBuilder.Entity<Empresa>().HasQueryFilter(e => e.TenantId == tenantId);
```

O EF Core faz cache do `IModel` por tipo de DbContext. Todos os requests subsequentes usam os valores do **primeiro** contexto instanciado (geralmente no startup/seed), causando vazamento de dados entre tenants.

**Correção:**
```csharp
// CORRETO — closure lê do provider a cada execução
modelBuilder.Entity<Empresa>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
```

---

### C2 — GrupoPreparo e PontoEntrega sem DbSet e sem Global Query Filters ✅

**Arquivo:** `OrionDbContext.cs` (ausência)
**Impacto:** Entidades EmpresaScoped sem proteção de filtragem

`DbSet<GrupoPreparo>` e `DbSet<PontoEntrega>` não estão declarados no DbContext. As configurações EF existem mas os global query filters para essas entidades nunca são registrados no `OnModelCreating`.

**Correção:** Declarar os DbSets e registrar os filtros no `OnModelCreating` seguindo o padrão das demais entidades EmpresaScoped.

---

### C3 — TipoAlmoxarifado sem Global Query Filter ✅

**Arquivo:** `OrionDbContext.cs:59-63`
**Impacto:** Entidade que implementa `ITenantAware` sem proteção

`TipoAlmoxarifado` implementa `ITenantAware` mas não tem global query filter registrado.

**Correção:** Verificar se `TipoAlmoxarifado` deve ser Global (catálogo compartilhado) ou EmpresaScoped. Se Global, remover `ITenantAware`. Se EmpresaScoped, adicionar filtro.

---

### C4 — TenantId não populado no SaveChangesAsync ✅

**Arquivo:** `OrionDbContext.cs:86-131`
**Impacto:** Registros gravados com `TenantId = ""`

O `SaveChangesAsync` popula `EmpresaId`, `CreatedAt`, `UpdatedAt`, `UserAgent`, `IpAddress`, mas **não popula `TenantId`**. Entidades tenant-scoped podem ser gravadas sem o isolamento correto.

**Correção:** Adicionar bloco para popular `TenantId` via `_tenantProvider.TenantId` no mesmo fluxo de auditoria automática.

---

### C5 — EnableSensitiveDataLogging sem condicional de ambiente ✅

**Arquivo:** `DatabaseConfiguration.cs:19`
**Impacto:** Dados sensíveis expostos em logs de produção

`EnableSensitiveDataLogging()` está ativo incondicionalmente.

**Correção:**
```csharp
if (environment.IsDevelopment())
    options.EnableSensitiveDataLogging();
```

---

### C6 — UpdateExtensions.ApplyNonNullChanges com reflection sem cache ✅

**Arquivo:** `UpdateExtensions.cs:10-36`
**Impacto:** Viola regra de performance — reflection por chamada em cada PUT

`GetProperties()` é chamado via reflection a cada operação de update, sem cache.

**Correção:** Adicionar cache via `ConcurrentDictionary<Type, PropertyInfo[]>` seguindo o mesmo padrão do `ObjectMapper`.

---

## 🟡 Desvios Menores — ✅ Maioria Corrigida em 2026-03-28

### M1 — Validators com `.When()` tornando campos obrigatórios opcionais

**Arquivos:** `CreateDepartamentosValidator.cs:20`, `CreateGruposPreparosValidator.cs:14`

`.When(x => x.Nome != null)` após `.NotEmpty()` faz com que a validação seja ignorada quando o campo é `null`, tornando campos "obrigatórios" opcionais.

---

### M2 — GruposPreparos e PontosEntregas não herdam de AuditableEntityConfiguration\<T\>

**Arquivos:** `Configurations/GruposPreparos.cs:7`, `Configurations/PontosEntregas.cs:7`

Usam `IEntityTypeConfiguration<T>` diretamente, perdendo o mapeamento centralizado de `created_at`, `updated_at`, `user_agent`, `ip_address`.

---

### M3 — Namespace placeholder não substituído

**Arquivos:** `GruposPreparos.cs:5`, `PontosEntregas.cs:5`

`YourNamespace.Infrastructure.Persistence.Configurations` em vez de `OrionERP.Infrastructure.Persistence.Configurations`.

---

### M4 — Ausência de índices nas entidades principais

Nenhuma configuração EF (exceto `RefreshTokenConfiguration`) define índices em `EmpresaId`, `TenantId` ou `Nome` — colunas filtradas frequentemente.

---

### M5 — Lógica de paginação duplicada em todos os GetAll handlers

Violação DRY. Candidato a extensão ou método base compartilhado.

---

### M6 — BaseCrudController.GetIdFromResponse() usa reflection sem cache

**Arquivo:** `BaseCrudController.cs:204`

Chamado a cada POST. Poderia ser resolvido com interface `IHasId<TId>` em `BaseResponse`.

---

### M7 — DTOs usam `partial class` em vez de `record`

**Arquivos:** `*Dto.cs`, `*Request.cs`

Regras do projeto especificam preferência por `record`, `required`, `init-only setters`.

---

### M8 — Typo no arquivo ValidattionBehavior.cs

**Arquivo:** `Common/Behaviors/ValidattionBehavior.cs`

Nome com duplo `t` — inconsistência de nomenclatura.

---

### M9 — Arquivos de configuração sem sufixo Configuration

**Arquivos:** `GruposPreparos.cs`, `PontosEntregas.cs`

Inconsistência com `DepartamentoConfiguration.cs`, `TipoAlmoxarifadoConfiguration.cs`.

---

### M10 — Ausência de testes para GruposPreparos e PontosEntregas

Testes unitários presentes apenas para Departamentos, Empresa e Auth.

---

### M11 — `virtual` em navigation properties sem lazy loading

`virtual ICollection<>` em entidades como `Departamento` sem `UseLazyLoadingProxies` habilitado — código sem propósito.

---

## ✅ Pontos Positivos

- Clean Architecture bem delimitada — sem vazamento entre camadas
- `ObjectMapper` com cache por expression tree — correto e eficiente
- `TenantProvider` com `AsyncLocal` — padrão correto para contexto por request
- `AuditableEntityConfiguration<T>` centraliza auditoria reutilizável
- `ValidationBehavior` registrado corretamente no pipeline MediatR
- `BaseCrudController<>` elimina duplicação de CRUD entre controllers
- Testes unitários presentes com NSubstitute + FluentAssertions
- `UtcDateTimeConverter` global via `ConfigureConventions` — consistência de timezone garantida
- Feature-based folder organization rigorosamente seguida em todos os módulos

---

## Prioridade de Correção — Status Final

| Ordem | Item                                               | Motivo                                       | Status                        |
| ----- | -------------------------------------------------- | -------------------------------------------- | ----------------------------- |
| 1     | C1 — Global Query Filters por closure              | Vazamento de dados entre tenants em produção | ✅ 2026-03-28                  |
| 2     | C2 — DbSets e filtros de GrupoPreparo/PontoEntrega | Entidades sem proteção                       | ✅ 2026-03-28                  |
| 3     | C4 — TenantId no SaveChangesAsync                  | Dados gravados sem isolamento                | ✅ 2026-03-28                  |
| 4     | C3 — TipoAlmoxarifado: definir escopo correto      | Inconsistência de design                     | ✅ 2026-03-28                  |
| 5     | C5 — EnableSensitiveDataLogging condicional        | Risco em produção                            | ✅ 2026-03-28                  |
| 6     | C6 — Cache no UpdateExtensions                     | Performance                                  | ✅ 2026-03-28                  |
| 7     | M1 — `.When()` em Create validators                | Campos obrigatórios tornados opcionais       | ✅ 2026-03-28                  |
| 8     | M2/M3/M9 — Config/namespace/sufixo                 | Inconsistência                               | ✅ Já estavam corretos         |
| 9     | M5 — Paginação duplicada                           | DRY violation                                | ✅ 2026-03-28                  |
| 10    | M6 — BaseCrudController reflection                 | Performance                                  | ✅ 2026-03-28                  |
| 11    | M8 — Typo ValidattionBehavior                      | Nomenclatura                                 | ✅ 2026-03-28                  |
| 12    | M11 — `virtual` sem lazy loading                   | Código sem propósito                         | ✅ 2026-03-28                  |
| —     | M4 — Índices                                       | Performance                                  | ⏳ Pendente (requer migration) |
| —     | M7 — DTOs como records                             | C# moderno                                   | ⏳ Pendente (impacto amplo)    |
| —     | M10 — Testes GruposPreparos/PontosEntregas         | Cobertura                                    | ⏳ Pendente                    |

## Hardening adicional aplicado (fora da auditoria original)

- `IDbContextFactory<OrionDbContext>` registrado — workers podem criar DbContext fora de scope HTTP
- `ITenantProvider.HasContext` adicionado — detecta estado não-inicializado
- Guard em `SaveChangesAsync` — lança `InvalidOperationException` em escritas sem contexto de tenant
- Documentação de processamento assíncrono criada: [[Processamento Assíncrono]]
