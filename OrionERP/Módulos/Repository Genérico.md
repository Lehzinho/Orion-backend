---
tags:
  - orionerp
  - repository
  - efcore
  - persistencia
  - paginacao
  - filtros
relacionado:
  - "[[Arquitetura Geral]]"
  - "[[Módulos/Multi-Tenancy]]"
  - "[[CQRS e MediatR]]"
  - "[[OrionDbContext]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# Repository Genérico

`IRepository<TEntity>` é o único ponto de acesso a dados do sistema. Injetado diretamente nos Handlers, elimina a necessidade de repositórios por entidade e centraliza a lógica de paginação, filtros dinâmicos e isolamento multi-tenant.

## Como funciona

O `Repository<TEntity>` implementa `IRepository<TEntity>` onde `TEntity : EntityBase`.

**Métodos disponíveis:**

| Método | Descrição |
|---|---|
| `GetByIdAsync(long id)` | Busca por PK |
| `GetAllAsync()` | Retorna todos (sem filtros) |
| `FindAsync(predicate)` | Busca por expressão lambda |
| `FindWithIncludeAsync(predicate, include)` | Busca com eager loading |
| `AsQueryable()` | Retorna `IQueryable` com filtro de tenant aplicado |
| `AsQueryableWithInclude(include)` | `IQueryable` com includes |
| `AddAsync(entity)` | Insere entidade |
| `AddRangeAsync(entities)` | Insere coleção |
| `Update(entity)` | Marca como modificada |
| `Remove(entity)` | Remove |
| `RemoveRange(entities)` | Remove coleção |
| `SaveChangesAsync()` | Persiste no banco |
| `GetPagedListAsync(page, size, filters, orderBy, includes)` | **Paginação com filtros e ordenação dinâmica** |

**Paginação (`GetPagedListAsync`):**
- Recebe `pageNumber`, `pageSize`, lista de `QueryFilter`, string `orderBy` (ex: `"Nome asc"`)
- Inclui eager loading via `params Expression<Func<TEntity, object>>[]`
- Ordenação padrão por `Id asc` se nenhuma for especificada
- Usa `System.Linq.Dynamic.Core` para ordenação dinâmica
- Usa `QueryParserService` para transformar `QueryFilter` em `Expression<Func<T, bool>>`
- Retorna `PaginatedResult<TEntity>` com metadados de paginação

**Filtro de tenant em `AsQueryable()`:**
```csharp
if (typeof(ITenantAware).IsAssignableFrom(typeof(TEntity)))
    query = query.Where(e => EF.Property<string>(e, "TenantId") == _tenantProvider.TenantId);
```

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Application/Common/Interfaces/IRepository.cs` | Contrato público |
| `OrionERP.Infrastructure/Persistence/Repositories/Repository.cs` | Implementação |
| `OrionERP.Application/Common/Models/PaginateResult.cs` | Modelo de resultado paginado |
| `OrionERP.Application/Common/Models/QueryFilter.cs` | Modelo de filtro dinâmico |
| `OrionERP.Application/Common/Models/FilterOperator.cs` | Enum de operadores (eq, ne, gt, lt, gte, lte, contains, startswith, endswith, between) |
| `OrionERP.Infrastructure/Services/QueryParserService.cs` | Transforma `QueryFilter` em expressões LINQ |

## Integrações

- Registrado como `Scoped` via `InfrastructureConfiguration`
- `IRepository<TEntity>` é injetado diretamente nos Handlers
- `OrionDbContext` aplica Global Query Filters complementares aos filtros manuais do Repository
- `SaveChangesAsync` delega para o `OrionDbContext`, que auto-popula campos de auditoria

## Configuração

Registrado em `InfrastructureConfiguration`:
```csharp
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

## Observações importantes

- **Nunca** criar repositórios específicos por entidade — viola o DRY e a decisão arquitetural
- Preferir `AsQueryable()` + composição antes de `GetAllAsync()` para evitar `ToList()` prematuro
- `GetPagedListAsync` é o método padrão para todos os endpoints de listagem
- O filtro de tenant em `AsQueryable()` é duplo (complementa o Global Query Filter do DbContext)
