# TODO — OrionERP: Roteiro de Implementação

> **Estado atual:** MVP `OrionPOS.*` com 4 endpoints básicos (health, cardápio, auth/login, auth/me)
> **Destino:** `OrionERP` multi-tenant, Clean Architecture, CQRS/MediatR, conforme documentado em `OrionERP/`
> **Referência principal:** `OrionERP/Orion-ERP.md` e `OrionERP/Módulos/`

---

## Fase 0 — Reestruturação da Solution

> Migrar do protótipo `OrionPOS.*` para a estrutura `OrionERP.*` documentada.

- [ ] Renomear solution de `OrionPOS.Backend.sln` para `OrionERP.sln`
- [ ] Renomear projetos:
  - `OrionPOS.Api` → `OrionERP.API`
  - `OrionPOS.Application` → `OrionERP.Application`
  - `OrionPOS.Domain` → `OrionERP.Domain`
  - `OrionPOS.Infra` → `OrionERP.Infrastructure`
- [ ] Adicionar projeto `OrionERP.Tests` (xUnit + Testcontainers)
- [ ] Organizar pastas conforme convenção:
  ```
  src/
  ├── OrionERP.API/
  ├── OrionERP.Application/
  ├── OrionERP.Domain/
  ├── OrionERP.Infrastructure/
  └── OrionERP.Tests/
  ```
- [ ] Atualizar referências de projeto no `.sln` e nos `.csproj`
- [ ] Verificar que a solução compila sem erros após renomeação

---

## Fase 1 — Infraestrutura Base

> Montar todos os blocos arquiteturais antes de qualquer módulo de negócio.
> Referência: `OrionERP/Módulos/`

### 1.1 EntityBase e Auditoria
> Ref: `OrionERP/Módulos/EntityBase e Auditoria.md`

- [ ] Criar `EntityBase` com campos: `Id (long)`, `CreatedAt`, `UpdatedAt`, `IsDeleted`
- [ ] Criar interface `ITenantAware` com `TenantId (string)` e `EmpresaId (long)`
- [ ] Fazer entidades domain herdarem de `EntityBase` onde aplicável

### 1.2 Multi-Tenancy
> Ref: `OrionERP/Módulos/Multi-Tenancy.md`

- [ ] Criar `ITenantProvider` em `Application/Common/Interfaces/`
- [ ] Implementar `TenantProvider` com `AsyncLocal<T>` em `Infrastructure/Services/`
- [ ] Implementar `TenantResolutionService` (resolve slug → Tenant + Empresa do banco)
- [ ] Criar `TenantMiddleware` em `API/Middlewares/` (extrai slug do host, popula contexto)
- [ ] Registrar middleware antes de `UseAuthentication` no `Program.cs`
- [ ] Registrar `ITenantProvider` como `Scoped` no DI

### 1.3 OrionDbContext
> Ref: `OrionERP/Módulos/OrionDbContext.md`

- [ ] Substituir `OrionPosDbContext` por `OrionDbContext` com suporte a `IDbContextFactory`
- [ ] Implementar Global Query Filters para entidades `TenantScoped` e `EmpresaScoped`
- [ ] Usar closure (não snapshot) nos filtros para leitura dinâmica a cada query
- [ ] Sobrescrever `SaveChangesAsync` para auto-popular: `TenantId`, `EmpresaId`, `CreatedAt`, `UpdatedAt`, `IpAddress`, `UserAgent`
- [ ] Lançar `InvalidOperationException` em `SaveChangesAsync` se `ITenantAware` sem contexto (`HasContext == false`)
- [ ] Migrar para EF Migrations (remover `DbInitializer` com SQL manual)

### 1.4 Repository Genérico
> Ref: `OrionERP/Módulos/Repository Genérico.md`

- [ ] Criar interface `IRepository<T>` em `Application/Common/Interfaces/`
  - Métodos: `GetByIdAsync`, `GetAllAsync` (paginado), `FindAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `FindWithIncludeAsync`
- [ ] Implementar `Repository<T>` em `Infrastructure/Repositories/`
  - Aplicar filtro manual de `TenantId` em queries para entidades `ITenantAware`
- [ ] Registrar `IRepository<>` no DI como genérico aberto

### 1.5 CQRS e MediatR
> Ref: `OrionERP/Módulos/CQRS e MediatR.md`

- [ ] Adicionar pacote NuGet `MediatR`
- [ ] Criar interfaces base em `Application/Common/Interfaces/`:
  - `ICommand<T>`, `ICreateCommand<T>`, `IUpdateCommand<TId, T>`, `IDeleteCommand<TId>`
  - `IQuery<T>`, `IGetByIdQuery<TId, TResponse>`, `IGetAllQuery<TResponse>`
- [ ] Criar modelo `PaginatedResult<T>` com `PageNumber`, `PageSize`, `TotalCount`, `Items`
- [ ] Registrar MediatR via `ApplicationExtensions.AddApplicationExtensions()`

### 1.6 ValidationBehavior (FluentValidation)
> Ref: `OrionERP/Módulos/ValidationBehavior.md`

- [ ] Adicionar pacote NuGet `FluentValidation.DependencyInjectionExtensions`
- [ ] Criar `ValidationBehavior<TRequest, TResponse>` como `IPipelineBehavior<,>`
  - Coleta todos `IValidator<TRequest>`, executa em paralelo, agrega erros
  - Lança `ValidationException` com lista de falhas se houver erros
- [ ] Registrar `ValidationBehavior` e validators via `AddValidatorsFromAssembly()`

### 1.7 ObjectMapper
> Ref: `OrionERP/Módulos/ObjectMapper.md`

- [ ] Implementar `ObjectMapper` baseado em Expression Trees (sem AutoMapper)
- [ ] Adicionar método de extensão `.MapTo<TResponse>()` para entidades e coleções
- [ ] Usar `ObjectMapper` nos handlers para projeção domain → DTO

### 1.8 BaseCrudController
> Ref: `OrionERP/Módulos/BaseCrudController.md`

- [ ] Criar `BaseCrudController<TEntity, TCreateRequest, TUpdateRequest, TResponse>` abstrato
  - Endpoints padrão: `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `DELETE /{id}`
  - Suporte a filtros por query string e paginação (`?page=1&pageSize=20`)
  - `[Authorize]` aplicado globalmente em todos os controllers derivados
  - Controllers enviam via `_mediator.Send()` — nunca acessam repositório diretamente

### 1.9 Exception Handling Middleware
> Ref: `OrionERP/Módulos/Exception Handling.md`

- [ ] Criar middleware `ExceptionHandlingMiddleware` em `API/Middlewares/`
- [ ] Tratar exceções conhecidas: `ValidationException` → 422, `NotFoundException` → 404, `UnauthorizedException` → 401
- [ ] Retornar `ProblemDetails` (RFC 7807) em todas as respostas de erro
- [ ] Nunca vazar stack trace em produção

### 1.10 Logging e Observabilidade
> Ref: `OrionERP/Módulos/Logging e Observabilidade.md`

- [ ] Adicionar pacote NuGet `Serilog.AspNetCore`
- [ ] Configurar sinks: Console (desenvolvimento) + Arquivo (produção)
- [ ] Logar request/response com correlation ID
- [ ] Incluir `TenantId` e `EmpresaId` nos logs via enriquecimento contextual

### 1.11 Swagger/OpenAPI
- [ ] Adicionar pacote NuGet `Swashbuckle.AspNetCore`
- [ ] Configurar Swagger UI em `/swagger`
- [ ] Configurar `SecurityDefinition` para Bearer JWT
- [ ] Anotar controllers com `[ProducesResponseType]` onde aplicável

### 1.12 Seed de Dados
> Ref: `OrionERP/Módulos/Seed de Dados.md`

- [ ] Migrar seed atual do `DbInitializer` para pattern de `IEntityTypeConfiguration` + `ModelBuilder.Entity<T>().HasData()`
- [ ] Separar seed de desenvolvimento do seed obrigatório de produção

---

## Fase 2 — Autenticação JWT Completa

> Ref: `OrionERP/Módulos/Autenticação JWT.md`

- [ ] Criar entidade `RefreshToken` (global, sem tenant scope) com campos: `Id`, `Token`, `TenantId`, `EmpresaId`, `UsuarioId`, `ExpiresAt`, `RevokedAt`
- [ ] Adicionar configuração EF para `RefreshToken` no `OrionDbContext`
- [ ] Implementar `LoginCommandHandler` com fluxo completo:
  - Valida tenant no contexto
  - Busca usuário por login + `TenantId`
  - Verifica senha com BCrypt
  - Valida status ativo do usuário
  - Busca `Empresa` e vínculo `EmpresaUsuario` para obter papel
  - Gera `AccessToken` JWT com claims: `usuarioId`, `nome`, `email`, `role`, `TenantId`, `TenantSlug`, `EmpresaId`, `EmpresaSlug`, `EmpresaCnpj`, `IsAdmin`
  - Persiste e retorna `RefreshToken`
- [ ] Implementar `RefreshAccessTokenCommandHandler` (`POST /api/auth/refresh`)
- [ ] Implementar `RevokeRefreshTokenCommandHandler` (`POST /api/auth/revoke`)
- [ ] Criar `LoginCommandValidator` (valida campos obrigatórios)
- [ ] Mover JWT key para variável de ambiente (`JWT__Secret`)
- [ ] Criar `AuthController` (não herda `BaseCrudController`)

---

## Fase 3 — CRUDs de Domínio Base

> Seguir o padrão documentado em `OrionERP/Módulos/Módulo Departamento.md` para cada módulo.
> Cada módulo exige: Entidade + Config EF + Commands + Queries + Validators + Models + Controller

### 3.1 Módulo Tenant
> Ref: `OrionERP/Módulos/Módulo Tenant.md`

- [ ] Entidade `Tenant` com campos: `Id (string/slug)`, `Nome`, `Slug`, `Ativo`, `CreatedAt`
- [ ] Config EF + DbSet + Seed básico
- [ ] CRUD completo (Tenant é global — sem tenant filter)

### 3.2 Módulo Empresa
> Ref: `OrionERP/Módulos/Módulo Empresa.md`

- [ ] Entidade `Empresa` com campos: `Id (long)`, `TenantId`, `Nome`, `Slug`, `Cnpj`, `Ativo`
- [ ] Config EF + DbSet + Global Query Filter (TenantScoped)
- [ ] CRUD completo + endpoint para listar empresas do tenant autenticado

### 3.3 Módulo Usuário
> Ref: `OrionERP/Módulos/Módulo Usuário.md`

- [ ] Entidade `Usuario` com campos: `Id (long)`, `TenantId`, `Login`, `PasswordHash`, `Nome`, `Ativo`, `CreatedAt`
- [ ] Config EF + DbSet + Global Query Filter (TenantScoped)
- [ ] CRUD completo (sem expor `PasswordHash` nas respostas)
- [ ] Endpoint para alterar senha

### 3.4 Módulo EmpresaUsuario
> Ref: `OrionERP/Módulos/Módulo EmpresaUsuario.md`

- [ ] Entidade `EmpresaUsuario` com campos: `Id (long)`, `TenantId`, `EmpresaId`, `UsuarioId`, `Papel (enum)`, `Ativo`
- [ ] Config EF + DbSet + Global Query Filters (TenantScoped + EmpresaScoped)
- [ ] CRUD completo (vincular/desvincular usuários de empresas com papéis)

### 3.5 Módulo Departamento (módulo de referência)
> Ref: `OrionERP/Módulos/Módulo Departamento.md`

- [ ] Entidade `Departamento` com campos: `Id (long)`, `EmpresaId`, `Nome`, `Ativo`
- [ ] Config EF + DbSet + Global Query Filter (EmpresaScoped)
- [ ] CRUD completo via `BaseCrudController`

### 3.6 Módulo Centro de Custo
> Ref: `OrionERP/Módulos/Módulo Centro de Custo.md`

- [ ] Entidade `CentroCusto` com campos: `Id (long)`, `EmpresaId`, `Nome`, `Codigo`, `Ativo`
- [ ] Config EF + DbSet + Global Query Filter (EmpresaScoped)
- [ ] CRUD completo via `BaseCrudController`

### 3.7 Módulo Almoxarifado
> Ref: `OrionERP/Módulos/Módulo Almoxarifado.md`

- [ ] Entidade `Almoxarifado` com campos: `Id (long)`, `EmpresaId`, `TipoAlmoxarifadoId`, `Nome`, `Ativo`
- [ ] Config EF + DbSet + Global Query Filter (EmpresaScoped)
- [ ] CRUD completo via `BaseCrudController`
- [ ] Validar que `TipoAlmoxarifadoId` existe antes de criar

### 3.8 Módulo Tipo de Almoxarifado
> Ref: `OrionERP/Módulos/Módulo Tipo de Almoxarifado.md`

- [ ] Entidade `TipoAlmoxarifado` com campos: `Id (long)`, `Nome`, `Ativo` (global)
- [ ] Config EF + DbSet (sem tenant filter — é catálogo global)
- [ ] CRUD completo via `BaseCrudController`

### 3.9 Módulo Grupo de Preparo
> Ref: `OrionERP/Módulos/Módulo Grupo de Preparo.md`

- [ ] Entidade `GrupoPreparo` com campos: `Id (long)`, `EmpresaId`, `Nome`, `Ativo`
- [ ] Config EF + DbSet + Global Query Filter (EmpresaScoped)
- [ ] CRUD completo via `BaseCrudController`

### 3.10 Módulo Ponto de Entrega
> Ref: `OrionERP/Módulos/Módulo Ponto de Entrega.md`

- [ ] Entidade `PontoEntrega` com campos: `Id (long)`, `EmpresaId`, `Nome`, `Tipo`, `Ativo`
- [ ] Config EF + DbSet + Global Query Filter (EmpresaScoped)
- [ ] CRUD completo via `BaseCrudController`

### 3.11 Módulo PDV
> Ref: `OrionERP/Módulos/Módulo PDV.md`
> Entidade e configuração EF já prontas — apenas CRUD pendente.

- [ ] Verificar/ajustar entidade `Pdv` e config EF existentes
- [ ] Criar Commands: `CreatePdvCommand`, `UpdatePdvCommand`, `DeletePdvCommand` + validators
- [ ] Criar Queries: `GetAllPdvQuery`, `GetPdvByIdQuery` + handlers
- [ ] Criar `PdvController` herdando `BaseCrudController`

---

## Fase 4 — Módulos de Negócio

### 4.1 Movimento / Turno / Caixa
> Ref: `OrionERP/Módulos/Módulo Movimento Turno Caixa.md` + `OrionERP/Research/Orion-ERP/movimentos-caixas-turnos/`

- [ ] Entidades: `Movimento`, `Turno`, `AuditoriaMovimento`
- [ ] Invariantes críticas:
  - Máximo 1 `Movimento` aberto por empresa (unique index condicional)
  - Não fechar `Movimento` com `Turnos` em aberto
  - Máximo 1 `Turno` aberto por PDV + Operador (unique index condicional)
  - Fechamento de `Movimento` gera `AuditoriaMovimento` via notification MediatR
- [ ] Endpoints: abrir/fechar movimento, abrir/fechar turno, consultar status
- [ ] Commands, Queries, Validators, Controller

### 4.2 Produtos & Cardápio
> Ref: `OrionERP/Research/Orion-ERP/produtos/produto_restaurante.md`

- [ ] Migrar entidades atuais de Menu (`MenuItem`, `MenuOptionGroup`, `MenuOption`) para o modelo OrionERP
- [ ] Adaptar para multi-tenancy (EmpresaScoped)
- [ ] Vincular `GrupoPreparo` aos produtos
- [ ] Commands, Queries, Validators, Controller completo (substituir endpoint `/cardapio` atual)

### 4.3 Pedidos e Vendas
> Ref: `OrionERP/Research/Orion-ERP/pedidos-vendas/pedido_venda.md`

- [ ] Entidade `PedidoVenda` com itens, status e fluxo de estados
- [ ] Vincular a `PontoEntrega`, `Turno`, `Operador`
- [ ] Commands: criar pedido, adicionar item, alterar status, cancelar
- [ ] Queries: listar pedidos por turno, por status, buscar por id
- [ ] Validators, Controller

### 4.4 Notas Fiscais (Fase 1 — Shell)
> Ref: `OrionERP/Research/Orion-ERP/pedidos-vendas/nota_fiscal.md`

- [ ] Criar estrutura base (entidade, config EF) sem emissão real (shell Phase 1)
- [ ] Endpoint para vincular NF a um pedido
- [ ] Emissão real (integração SEFAZ) fica para Fase 2

### 4.5 Comanda / Mesa
> Ref: `OrionERP/Research/Orion-ERP/pedidos-vendas/comanda.md`

- [ ] Entidades: `Comanda`, vínculo com `PontoEntrega` (tipo mesa)
- [ ] Fluxo: abrir comanda → adicionar itens → fechar comanda → gerar pedido
- [ ] Commands, Queries, Validators, Controller

### 4.6 Cartões Internos
> Ref: `OrionERP/Research/Orion-ERP/cartoes-internos/`
> Especificação em construção.

- [ ] Aguardar finalização da especificação em `OrionERP/Research/Orion-ERP/cartoes-internos/`
- [ ] Implementar após especificação aprovada

### 4.7 Estoque
> Ref: `OrionERP/Research/Orion-ERP/estoque/estoque_nfe_spec.md`
> Especificação em construção.

- [ ] Aguardar finalização da especificação em `OrionERP/Research/Orion-ERP/estoque/`
- [ ] Implementar após especificação aprovada

---

## Fase 5 — Evoluções Estruturais

- [ ] **Soft Delete**: adicionar campo `DeletedAt` em `EntityBase`, aplicar filtro global `e.DeletedAt == null` para todas as entidades
- [ ] **Auditoria por usuário**: adicionar `CreatedBy (long?)` e `UpdatedBy (long?)` em `EntityBase`, auto-popular via `ITenantProvider.UsuarioId` no `SaveChangesAsync`
- [ ] **Permissões avançadas via EmpresaUsuario**: criar sistema de permissões granulares por papel, validar permissões nos handlers críticos
- [ ] **Processamento Assíncrono**: implementar suporte a workers/filas conforme `OrionERP/Módulos/Processamento Assíncrono.md`

---

## Fase 6 — Testes
> Ref: `OrionERP/Módulos/Testes.md`

- [ ] Configurar projeto `OrionERP.Tests` com xUnit
- [ ] Adicionar `Testcontainers.PostgreSql` para testes de integração com banco real
- [ ] Testes unitários dos use cases/handlers (mockar `IRepository<T>` e `ITenantProvider`)
- [ ] Testes de integração para todos os módulos (Fase 3 e 4)
- [ ] Testes multi-tenant: garantir isolamento — tenant A não acessa dados do tenant B
- [ ] Testes de validação: garantir que FluentValidation rejeita inputs inválidos
- [ ] Testes de regras de negócio: invariantes de Movimento/Turno/Caixa

---

## Fase 7 — Segurança e Performance

- [ ] **Rate Limiting**: configurar `AspNetCoreRateLimit` ou `Microsoft.AspNetCore.RateLimiting` (sliding window por IP + por tenant)
- [ ] **Caching**: adicionar `IMemoryCache` ou `IDistributedCache` para queries de catálogos (TipoAlmoxarifado, GrupoPreparo, etc.)
- [ ] **Índices otimizados**: revisar e adicionar índices no banco para queries frequentes (por `TenantId`, `EmpresaId`, `Slug`)
- [ ] **CORS**: configurar política de CORS para domínios permitidos
- [ ] **Segredos**: mover todas as configurações sensíveis (JWT Secret, connection string) para variáveis de ambiente e/ou `dotnet user-secrets`
- [ ] **appsettings.Production.json**: criar arquivo de configuração para produção

---

## Fase 8 — Infraestrutura de Deploy

- [ ] Criar `Dockerfile` para `OrionERP.API`
- [ ] Criar `docker-compose.yml` com API + PostgreSQL
- [ ] Criar `.env.example` com variáveis necessárias
- [ ] Configurar `healthcheck` real no endpoint `/health` (verificar conexão com banco)
- [ ] Configurar `appsettings.Production.json` com valores de produção
- [ ] Criar pipeline CI/CD (GitHub Actions ou similar): build → test → docker build → push

---

## Referência Rápida de Módulos

| Módulo | Fase | Especificação | Status |
|---|---|---|---|
| Infraestrutura Base | 1 | `Módulos/` | Pendente |
| JWT + Refresh Token | 2 | `Módulos/Autenticação JWT.md` | Parcial (sem refresh) |
| Tenant | 3.1 | `Módulos/Módulo Tenant.md` | Pendente |
| Empresa | 3.2 | `Módulos/Módulo Empresa.md` | Pendente |
| Usuário | 3.3 | `Módulos/Módulo Usuário.md` | Pendente |
| EmpresaUsuario | 3.4 | `Módulos/Módulo EmpresaUsuario.md` | Pendente |
| Departamento | 3.5 | `Módulos/Módulo Departamento.md` | Pendente |
| Centro de Custo | 3.6 | `Módulos/Módulo Centro de Custo.md` | Pendente |
| Almoxarifado | 3.7 | `Módulos/Módulo Almoxarifado.md` | Pendente |
| Tipo de Almoxarifado | 3.8 | `Módulos/Módulo Tipo de Almoxarifado.md` | Pendente |
| Grupo de Preparo | 3.9 | `Módulos/Módulo Grupo de Preparo.md` | Pendente |
| Ponto de Entrega | 3.10 | `Módulos/Módulo Ponto de Entrega.md` | Pendente |
| PDV | 3.11 | `Módulos/Módulo PDV.md` | Entidade pronta, CRUD pendente |
| Movimento/Turno/Caixa | 4.1 | `Research/.../movimentos-caixas-turnos/` | Pendente |
| Produtos & Cardápio | 4.2 | `Research/.../produtos/` | MVP parcial |
| Pedidos e Vendas | 4.3 | `Research/.../pedidos-vendas/` | Pendente |
| Notas Fiscais | 4.4 | `Research/.../nota_fiscal.md` | Pendente |
| Comanda / Mesa | 4.5 | `Research/.../comanda.md` | Pendente |
| Cartões Internos | 4.6 | `Research/.../cartoes-internos/` | Spec em construção |
| Estoque | 4.7 | `Research/.../estoque/` | Spec em construção |
| Soft Delete | 5 | — | Pendente |
| Auditoria CreatedBy | 5 | — | Pendente |
| Testes | 6 | `Módulos/Testes.md` | Pendente |
| Rate Limiting | 7 | — | Pendente |
| Caching | 7 | — | Pendente |
| Docker / Deploy | 8 | — | Pendente |

---

## Como Adicionar Nova Feature

> Seguir o padrão do `Módulo Departamento` como referência.

1. **Domain** → entidade + `ITenantAware` se necessário
2. **Infrastructure** → `IEntityTypeConfiguration<T>` + `DbSet` no `OrionDbContext` + Global Query Filter se EmpresaScoped
3. **Application** → Commands, Queries, Models, Validators dentro de `Features/NomeDaFeature/`
4. **API** → Controller herdando `BaseCrudController<>`

Sempre validar: **DRY · KISS · YAGNI · Segurança multi-tenant**
