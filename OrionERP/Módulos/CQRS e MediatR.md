---
tags: [orionerp, cqrs, mediatr, commands, queries, handlers, validators]
relacionado: 
    - "[[Arquitetura Geral]]"
    - "[[BaseCrudController]]" 
    - "[[Repository Genérico]]" 
    - "[[ValidationBehavior]]"
status: ativo
tipo: arquitetura
versao: 1.0.0
---

# CQRS e MediatR

Padrão de separação entre leitura e escrita implementado via MediatR. Cada operação de negócio tem um Command ou Query dedicado com seu Handler. Validators são executados automaticamente pelo pipeline.

## Como funciona

**Commands** — modificam estado (Create, Update, Delete):
- Implementam `IRequest<TResponse>` ou `ICreateCommand<T>` / `IUpdateCommand<TId, T>` / `IDeleteCommand<TId>`
- Executados pelo Handler correspondente
- Validados automaticamente pelo `ValidationBehavior` antes de chegar ao Handler

**Queries** — apenas leitura:
- Implementam `IGetByIdQuery<TId, TResponse>` ou `IGetAllQuery<TResponse>`
- `GetAll` recebe `PageNumber`, `PageSize`, `OrderBy`, `Filters`
- Retornam `PaginatedResult<T>` ou entidade direta

**Pipeline MediatR:**
```
Request → ValidationBehavior → Handler → Response
```

O `ValidationBehavior` coleta todos os `IValidator<TRequest>` registrados, executa em paralelo e lança `ValidationException` com todos os erros agregados se houver falhas.

**Convenção de pastas por feature:**
```
Features/
└── NomeDaFeature/
    ├── Commands/
    │   ├── CreateXyzCommandHandler.cs
    │   ├── CreateXyzValidator.cs
    │   ├── UpdateXyzCommandHandler.cs
    │   ├── UpdateXyzValidator.cs
    │   └── DeleteXyzCommandHandler.cs
    ├── Queries/
    │   ├── GetAllXyzQuery.cs
    │   ├── GetAllXyzQueryHandler.cs
    │   ├── GetXyzByIdQuery.cs
    │   └── GetXyzByIdQueryHandler.cs
    └── Models/
        ├── XyzDto.cs
        ├── XyzResponse.cs
        ├── CreateXyzRequest.cs
        ├── UpdateXyzRequest.cs
        └── DeleteXyzRequest.cs
```

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Application/Common/Behaviors/ValidattionBehavior.cs` | Pipeline que executa validators antes dos handlers |
| `OrionERP.Application/Common/Interfaces/ICommand.cs` | Interfaces base de Commands |
| `OrionERP.Application/Common/Interfaces/IQuery.cs` | Interfaces base de Queries |
| `OrionERP.Application/Extensions/ApplicationExtensions.cs` | Registro de MediatR, Validators e Behaviors no DI |

## Integrações

- **FluentValidation** — validators implementam `AbstractValidator<TCommand>`
- **[[BaseCrudController]]** — controllers constroem Commands/Queries e enviam via `_mediator.Send()`
- **[[Repository Genérico]]** — handlers injetam `IRepository<TEntity>` para acesso a dados
- **[[ObjectMapper]]** — handlers usam `.MapTo<TResponse>()` para projeção

## Configuração

Registrado em `ApplicationExtensions.AddApplicationExtensions()`:
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

## Observações importantes

- Handlers devem orquestrar — não conter lógica de negócio complexa
- Nunca acessar dados diretamente no Controller, sempre via MediatR
- Validators vivem dentro da feature, nunca em pasta global
- Commands não retornam dados de leitura — para isso, há a Query correspondente
- `GetAll` sempre pagina — nunca retorna lista sem limite
