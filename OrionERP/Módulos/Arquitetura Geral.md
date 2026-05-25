---
tags:
  - orionerp
  - arquitetura
  - clean-architecture
  - csharp
  - dotnet9
status: ativo
tipo: arquitetura
versao: 1.0.0
relacionado:
  - "[[Módulo Departamento]]"
  - "[[CQRS e MediatR]]"
---

# Arquitetura Geral

OrionERP é uma REST API multi-tenant construída em .NET 9 seguindo Clean Architecture com 4 camadas. Substitui um legado Delphi + PostgreSQL e tem como foco inicial o MVP de PDV para restaurantes.

## Como funciona

A solução é dividida em 4 projetos com dependência unidirecional: `API → Application → Domain`, e `Infrastructure → Application`.

```
OrionERP.API           → HTTP, Controllers, Middlewares
OrionERP.Application   → CQRS, Handlers, Validators, DTOs, Interfaces
OrionERP.Domain        → Entities, EntityBase, ITenantAware
OrionERP.Infrastructure → EF Core, Repository, Services, Seeds
OrionERP.Tests         → Unitários e de Integração
```

**Regra de dependência:** camadas externas dependem das internas. O Domain não tem dependências externas.

**Princípios ativos:**
- DRY · KISS · YAGNI em toda adição
- Explícito > mágico
- Performance > conveniência

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.API/Program.cs` | Bootstrap da aplicação |
| `OrionERP.Application/Extensions/ApplicationExtensions.cs` | Registro de MediatR, Validators, Behaviors |
| `OrionERP.Infrastructure/Configurations/InfrastructureConfiguration.cs` | Registro de serviços de infraestrutura |
| `OrionERP.Infrastructure/Configurations/DatabaseConfiguration.cs` | Configuração do EF Core + PostgreSQL |

## Integrações

- **EF Core + PostgreSQL** via `Npgsql`
- **MediatR** para CQRS
- **FluentValidation** com pipeline behavior
- **Serilog** para logging estruturado
- **JWT** para autenticação
- **Swagger/OpenAPI** para documentação

## Configuração

A API escuta em `http://0.0.0.0:5000` no ambiente de desenvolvimento.

```bash
# Build
dotnet build OrionERP.sln

# Run
dotnet run --project OrionERP.API/OrionERP.API.csproj

# Migration
dotnet ef migrations add <Nome> --project ../OrionERP.Infrastructure --startup-project .

# Apply
dotnet ef database update --project ../OrionERP.Infrastructure --startup-project .
```

## Observações importantes

- Nunca introduzir AutoMapper, UnitOfWork extra, ou vertical slicing frameworks
- Nunca burlar o [[Módulos/Multi-Tenancy|TenantProvider]]
- `BaseCrudController<>`, `IRepository<T>`, `ObjectMapper`, `TenantProvider` e `Global Query Filters` são contratos estáveis — mudanças exigem justificativa arquitetural explícita
