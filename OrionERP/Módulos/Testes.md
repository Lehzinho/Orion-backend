---
tags: [orionerp, testes, unitarios, integracao, xunit]
relacionado: 
    - "[[Arquitetura Geral]]"
    - "[[CQRS e MediatR]]" 
    - "[[Autenticação JWT]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# Testes

Projeto `OrionERP.Tests` com testes unitários e de integração. Testa comportamento, não implementação. Mocks via `Moq`, assertions via `FluentAssertions`.

## Como funciona

**Estrutura:**
```
OrionERP.Tests/
├── Unit/
│   ├── Application/
│   │   ├── Common/
│   │   │   ├── Behaviors/ValidationBehaviorTests.cs
│   │   │   └── Mapping/ObjectMapperTests.cs
│   │   └── Features/
│   │       ├── Auth/
│   │       │   ├── LoginCommandHandlerTests.cs
│   │       │   ├── RefreshAccessTokenHandlerTests.cs
│   │       │   └── RevokeRefreshTokenHandlerTests.cs
│   │       ├── Departamento/
│   │       │   ├── CreateDepartamentoCommandHandlerTests.cs
│   │       │   ├── DeleteDepartamentoCommandHandlerTests.cs
│   │       │   ├── GetDepartamentoByIdQueryHandlerTests.cs
│   │       │   └── Validators/CreateDepartamentosValidatorTests.cs
│   │       └── Empresa/
│   │           ├── CreateEmpresaCommandHandlerTests.cs
│   │           ├── DeleteEmpresaCommandHandlerTests.cs
│   │           ├── GetAllEmpresasQueryHandlerTests.cs
│   │           ├── GetEmpresaByIdQueryHandlerTests.cs
│   │           └── Validators/CreateEmpresaValidatorTests.cs
│   └── Infrastructure/
│       └── Services/
│           ├── PasswordHasherServiceTests.cs
│           ├── QueryParserServiceTests.cs
│           ├── QueryStringParserServiceTests.cs
│           └── TokenServiceTests.cs
└── Integration/
    ├── API/
    │   ├── AuthEndpointsTests.cs
    │   ├── EmpresaEndpointsTests.cs
    │   └── Middleware/ExceptionHandlingMiddlewareTests.cs
    └── Fixtures/
        ├── IntegrationTestCollection.cs
        └── TestWebApplicationFactory.cs
```

**Testes unitários:**
- Mocam `IRepository<T>`, `ITenantProvider`, `ITokenService`, etc. via Moq
- Testam o comportamento do handler isolado de infraestrutura
- Cobrem cenários de sucesso e erro (entidade não encontrada, validação, etc.)

**Testes de integração:**
- Usam `TestWebApplicationFactory` derivada de `WebApplicationFactory<Program>`
- Sobrescrevem serviços de banco para testes (SQLite in-memory ou PostgreSQL de teste)
- Testam fluxos completos: request HTTP → middleware → handler → banco → response

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Tests/Integration/Fixtures/TestWebApplicationFactory.cs` | Factory para testes de integração com banco real/in-memory |
| `OrionERP.Tests/Integration/Fixtures/IntegrationTestCollection.cs` | Coleção xUnit para compartilhar factory |
| `OrionERP.Tests/Unit/Application/Common/Behaviors/ValidationBehaviorTests.cs` | Testa o pipeline de validação |
| `OrionERP.Tests/Unit/Application/Common/Mapping/ObjectMapperTests.cs` | Testa o ObjectMapper |

## Integrações

- `dotnet test` executa todos os testes
- Filtro `--filter "FullyQualifiedName~Unit"` para apenas unitários
- Filtro `--filter "FullyQualifiedName~Integration"` para apenas integração

## Configuração

```bash
# Executar da pasta src/
dotnet test OrionERP.Tests/OrionERP.Tests.csproj

# Apenas unitários
dotnet test OrionERP.Tests/OrionERP.Tests.csproj --filter "FullyQualifiedName~Unit"

# Apenas integração
dotnet test OrionERP.Tests/OrionERP.Tests.csproj --filter "FullyQualifiedName~Integration"
```

## Observações importantes

- **Não mockar o banco nos testes de integração** — hit real em banco de testes
- Testar comportamento, não implementação
- Cobertura atual: Auth (login, refresh, revoke), Empresa CRUD, Departamento CRUD, validators, ObjectMapper, TokenService, QueryParserService, PasswordHasher, ExceptionMiddleware
- Cobertura pendente: demais módulos CRUD (Almoxarifado, CentroCusto, TipoAlmoxarifado, GrupoPreparo, PontoEntrega), fluxos multi-tenant, autorização
