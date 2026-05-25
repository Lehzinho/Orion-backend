---
tags: [orionerp, multi-tenancy, segurança, middleware, tenant]
relacionado:
    - "[[Arquitetura Geral]]"
    - "[[Empresa]]"
    - "[[OrionDbContext]]"
    - "[[CQRS e MediatR]]"
    - "[[Processamento Assíncrono]]"
status: ativo
tipo: arquitetura
versao: 1.1.0
updated: 2026-03-28
---

# Multi-Tenancy

Implementação de isolamento multi-tenant baseada em subdomínio (host-based slug strategy). Cada requisição é associada a um `Tenant` e uma `Empresa` antes de qualquer lógica de negócio. É a regra mais crítica do sistema — nunca pode ser burlada.

## Como funciona

**Fluxo completo por requisição HTTP:**

1. `TenantMiddleware` extrai o slug do hostname: `empresaA.orionerp.com` → slug `empresaA`
2. `ITenantResolutionService` busca o `Tenant` e a `Empresa` correspondentes no banco
3. `ITenantProvider.SetCurrentTenantContext(...)` armazena `TenantId`, `EmpresaId`, `Slug`, `UserAgent` e `IpAddress` via `AsyncLocal<T>` (sem vazamento entre threads)
4. `OrionDbContext` lê o contexto do `ITenantProvider` via propriedades `CurrentTenantId` e `CurrentEmpresaId` — avaliados a cada query, não capturados no model cache
5. `SaveChangesAsync` auto-popula `TenantId`, `EmpresaId`, `CreatedAt`, `UpdatedAt`, `IpAddress` e `UserAgent` em todas as entidades

**Global Query Filters aplicados no `OrionDbContext`:**

Os filtros usam closures que leem do provider a cada execução — não capturam valores por snapshot:

```csharp
// TenantScoped — null libera filtro (migrations/seed/workers sem contexto de leitura)
e.TenantId == CurrentTenantId (quando CurrentTenantId != null)

// EmpresaScoped — 0 libera filtro
e.EmpresaId == CurrentEmpresaId (quando CurrentEmpresaId != 0)
```

| Filtro | Entidades afetadas |
|---|---|
| TenantScoped | Empresa, Usuario |
| EmpresaScoped | Almoxarifado, CentroCusto, Departamento, GrupoPreparo, PontoEntrega, Pdv, TipoAlmoxarifado |

**Escopo Multi-Tenant por entidade:**

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
| RefreshToken | ❌ | ❌ | ✅ |

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.API/Middlewares/TenantMiddleware.cs` | Extrai slug do host, resolve tenant, popula contexto |
| `OrionERP.Infrastructure/Services/TenantProvider.cs` | Armazena e expõe contexto via `AsyncLocal<T>` |
| `OrionERP.Infrastructure/Services/TenantResolutionService.cs` | Busca Tenant + Empresa pelo slug no banco |
| `OrionERP.Application/Common/Interfaces/ITenantProvider.cs` | Contrato de acesso ao contexto (Application layer) |
| `OrionERP.Domain/Common/Interfaces/ITenantAware.cs` | Interface que entidades tenant-scoped devem implementar |
| `OrionERP.Infrastructure/Persistence/OrionDbContext.cs` | Aplica query filters e auto-popula campos de auditoria |

## Integrações

- Todo handler que cria/atualiza entidades EmpresaScoped depende do `ITenantProvider` via `SaveChangesAsync`
- `LoginCommandHandler` usa `ITenantProvider` para validar tenant antes do login
- O `Repository<T>` aplica filtro manual de `TenantId` em `AsQueryable()` e `FindWithIncludeAsync()` para entidades `ITenantAware`

## Configuração

O `TenantMiddleware` é registrado via `app.RegisterMiddlewares()` no `Program.cs`, antes de `UseAuthentication`.

O `ITenantProvider` é registrado como `Scoped` na `InfrastructureConfiguration`.

## `ITenantProvider` — Contrato

```csharp
string TenantId { get; }
long EmpresaId { get; }
string TenantSlug { get; }
string? UserAgent { get; }
string? IpAddress { get; }
long? UsuarioId { get; }
bool IsAuthenticated { get; }
bool HasContext { get; }   // true quando SetCurrentTenantContext já foi chamado com TenantId válido

void SetCurrentTenantContext(string tenantId, long empresaId, string tenantSlug, string? userAgent, string? ipAddress);
void SetUsuario(long usuarioId);
```

`HasContext` permite detectar se o contexto está inicializado — essencial em workers e background services.

## Processamento em Background (Workers / Filas / Eventos)

`TenantProvider` usa `AsyncLocal<T>` — **não depende de `HttpContext`**. Isso significa que pode ser usado fora do ciclo HTTP desde que o contexto seja inicializado manualmente.

Ver padrão completo em: [[Processamento Assíncrono]]

**Regra para qualquer worker que opere sobre entidades tenant-scoped:**

```csharp
// 1. Criar escopo próprio (workers são Singleton, serviços são Scoped)
using var scope = _scopeFactory.CreateScope();

// 2. Resolver serviços
var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
var repository = scope.ServiceProvider.GetRequiredService<IRepository<MinhaEntidade>>();

// 3. OBRIGATÓRIO: bootstrapar contexto antes de qualquer operação de DB
// TenantId e EmpresaId vêm do payload do job/mensagem, não do HTTP
tenantProvider.SetCurrentTenantContext(tenantId, empresaId, slug, userAgent: null, ipAddress: null);

// 4. Operações normais — filtros e auditoria funcionam corretamente
```

**Guard de segurança:** `SaveChangesAsync` lança `InvalidOperationException` se houver entidade `ITenantAware` para gravar e `HasContext == false`. Previne gravação de dados sem isolamento de tenant.

## Observações importantes

- **Nunca** usar `IgnoreQueryFilters()` sem justificativa arquitetural explícita
- **Nunca** setar `TenantId` ou `EmpresaId` manualmente em handlers — o `SaveChangesAsync` cuida disso
- **Nunca** burlar o `TenantProvider` diretamente
- Se uma nova entidade é criada, verificar: precisa de `ITenantAware`? Precisa de `EmpresaId`? Precisa de global filter?
- O `AsyncLocal<T>` garante que o contexto é thread-safe e flui corretamente através de `await`
- Em workers, **sempre** chamar `SetCurrentTenantContext` antes de qualquer operação de leitura ou escrita
