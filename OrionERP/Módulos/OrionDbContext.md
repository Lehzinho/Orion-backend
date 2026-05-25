---
tags:
  - orionerp
  - efcore
  - dbcontext
  - persistencia
  - auditoria
  - migrations
relacionado:
  - "[[Módulos/Multi-Tenancy]]"
  - "[[Repository Genérico]]"
  - "[[Arquitetura Geral]]"
  - "[[Processamento Assíncrono]]"
status: ativo
tipo: componente
versao: 1.1.0
updated: 2026-03-28
---

# OrionDbContext

DbContext central do sistema. Responsável pelos DbSets, aplicação de Global Query Filters de multi-tenancy, conversão de datas para UTC e auto-população de campos de auditoria no `SaveChangesAsync`.

## DbSets registrados

```csharp
DbSet<Almoxarifado>      Almoxarifados
DbSet<CentroCusto>       CentrosCustos
DbSet<Departamento>      Departamentos
DbSet<Empresa>           Empresas
DbSet<EmpresaUsuario>    EmpresasUsuarios
DbSet<GrupoPreparo>      GruposPreparos
DbSet<Pdv>               Pdvs
DbSet<PontoEntrega>      PontosEntregas
DbSet<Tenant>            Tenants
DbSet<TipoAlmoxarifado>  TiposAlmoxarifados
DbSet<Usuario>           Usuarios
```

## Propriedades de contexto

```csharp
public string? CurrentTenantId => _tenantProvider?.TenantId;
public long CurrentEmpresaId   => _tenantProvider?.EmpresaId ?? 0;
```

Avaliadas a cada execução de query — **não capturam valor por snapshot no model cache**. Esta é a correção crítica que evita vazamento cross-tenant (bug C1 da auditoria 2026-03).

## Global Query Filters (OnModelCreating)

Filtros registrados como closures que leem das propriedades acima a cada query:

```csharp
// TenantScoped — null = sem contexto, libera filtro (migrations/seed/workers de leitura)
modelBuilder.Entity<Empresa>().HasQueryFilter(e => CurrentTenantId == null || e.TenantId == CurrentTenantId);
modelBuilder.Entity<Usuario>().HasQueryFilter(u => CurrentTenantId == null || u.TenantId == CurrentTenantId);

// EmpresaScoped — 0 = sem contexto, libera filtro
modelBuilder.Entity<Almoxarifado>().HasQueryFilter(a => CurrentEmpresaId == 0 || a.EmpresaId == CurrentEmpresaId);
modelBuilder.Entity<CentroCusto>().HasQueryFilter(c => CurrentEmpresaId == 0 || c.EmpresaId == CurrentEmpresaId);
modelBuilder.Entity<Departamento>().HasQueryFilter(d => CurrentEmpresaId == 0 || d.EmpresaId == CurrentEmpresaId);
modelBuilder.Entity<GrupoPreparo>().HasQueryFilter(g => CurrentEmpresaId == 0 || g.EmpresaId == CurrentEmpresaId);
modelBuilder.Entity<PontoEntrega>().HasQueryFilter(p => CurrentEmpresaId == 0 || p.EmpresaId == CurrentEmpresaId);
modelBuilder.Entity<Pdv>().HasQueryFilter(p => CurrentEmpresaId == 0 || p.EmpresaId == CurrentEmpresaId);
modelBuilder.Entity<TipoAlmoxarifado>().HasQueryFilter(t => CurrentEmpresaId == 0 || t.EmpresaId == CurrentEmpresaId);
```

**Configurações EF:** todas as `IEntityTypeConfiguration<T>` são descobertas via `ApplyConfigurationsFromAssembly`. Convenção: snake_case para tabelas e colunas. Conversão UTC em todas as `DateTime` via `UtcDateTimeConverter`.

## Auto-população em SaveChangesAsync

Para todas as entidades em estado `Added` ou `Modified`:

| Campo | Quando | Fonte |
|---|---|---|
| `UpdatedAt` | Added + Modified | `DateTime.UtcNow` |
| `CreatedAt` | Added | `DateTime.UtcNow` |
| `TenantId` | Added + Modified | `ITenantProvider.TenantId` |
| `EmpresaId` | Added + Modified | `ITenantProvider.EmpresaId` |
| `UserAgent` | Added + Modified | `ITenantProvider.UserAgent` |
| `IpAddress` | Added + Modified | `ITenantProvider.IpAddress` |

### Guard de segurança em SaveChangesAsync

Se houver entidade `ITenantAware` para gravar e `ITenantProvider.HasContext == false`, lança `InvalidOperationException`:

```
"Tentativa de gravar entidade ITenantAware sem contexto de tenant inicializado.
Em workers e background services, chame ITenantProvider.SetCurrentTenantContext() antes de qualquer operação de escrita."
```

Protege contra gravação de dados sem isolamento em workers mal configurados.

## Registro em DI

```csharp
// HTTP requests (Scoped — 1 instância por request)
services.AddDbContext<OrionDbContext>(...);

// Workers e background services (factory — cria instância sob demanda)
services.AddDbContextFactory<OrionDbContext>(...);
```

Ambos usam as mesmas opções de configuração. `AddDbContext` e `AddDbContextFactory` coexistem sem conflito.

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Infrastructure/Persistence/OrionDbContext.cs` | Contexto principal |
| `OrionERP.Infrastructure/Persistence/Configurations/*.cs` | Configurações EF por entidade |
| `OrionERP.Infrastructure/Configurations/DatabaseConfiguration.cs` | Registro em DI |
| `OrionERP.Infrastructure/Persistence/Migrations/Domain/` | Migrations geradas |

## Integrações

- [[Módulos/Multi-Tenancy]] — recebe `ITenantProvider` via construtor para aplicar filtros e auditoria
- [[Repository Genérico]] — `Repository<T>` recebe o `OrionDbContext` via DI
- [[Processamento Assíncrono]] — `IDbContextFactory<OrionDbContext>` disponível para workers

## Observações importantes

- Todas as `DateTime` convertidas para UTC — consistência garantida
- Os Global Query Filters são avaliados por execução de query (closure), não por model cache — sem risco de snapshot stale
- Cada requisição HTTP tem instância própria de `OrionDbContext` (Scoped)
- Workers devem usar `IDbContextFactory<OrionDbContext>` ou `IServiceScopeFactory` — ver [[Processamento Assíncrono]]
- Para migrations CLI, usar o `OrionDbContextFactory` que cria contexto sem `ITenantProvider` (parâmetro nullable)
