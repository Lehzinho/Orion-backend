---
tags:
  - orionerp
  - controller
  - crud
  - api
  - paginacao
  - filtros
relacionado:
  - "[[CQRS e MediatR]]" 
  - "[[Arquitetura Geral]]" 
  - "[[Autenticação JWT]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# BaseCrudController

Controller genérico abstrato que implementa os 5 endpoints CRUD padrão para qualquer entidade. Todos os controllers de domínio herdam desta classe, eliminando duplicação de código HTTP.

## Como funciona

`BaseCrudController<TId, TResponse, TCreateCommand, TUpdateCommand, TGetByIdQuery, TGetAllQuery, TDeleteCommand>` é parametrizado por tipos que representam as operações da feature.

**Endpoints gerados automaticamente:**

| Método | Rota | Descrição |
|---|---|---|
| `POST /` | Criar entidade | Executa `TCreateCommand`, retorna `201 Created` |
| `GET /{id}` | Buscar por ID | Executa `TGetByIdQuery`, retorna `200 OK` ou `404` |
| `GET /` | Listar paginado | Executa `TGetAllQuery`, retorna `200 OK` com headers de paginação |
| `PUT /{id}` | Atualizar | Executa `TUpdateCommand`, retorna `200 OK` |
| `DELETE /{id}` | Excluir | Executa `TDeleteCommand`, retorna `204 No Content` |

**Sistema de filtros no `GET /`:**

Os filtros são passados como query parameters no formato `PropName.operator=value`:
```
GET /api/departamentos?Nome.contains=TI&Ativo.eq=true&CreatedAt.gte=2026-01-01&orderBy=Nome asc&pageNumber=1&pageSize=20
```

Operadores suportados: `eq`, `ne`, `gt`, `lt`, `gte`, `lte`, `contains`, `startswith`, `endswith`, `between`.

**Headers de paginação** retornados no `GET /`:
- `X-Pagination` com JSON contendo `TotalCount`, `PageSize`, `CurrentPage`, `TotalPages`, `HasNext`, `HasPrevious`, `NextPage`, `PreviousPage`

Todos os endpoints exigem autenticação via `[Authorize]`.

**Exemplo de controller concreto:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class DepartamentosController : BaseCrudController<
    long, DepartamentosResponse,
    CreateDepartamentosCommandHandler, UpdateDepartamentosCommandHandler,
    GetDepartamentosByIdQuery, GetAllDepartamentosQuery,
    DeleteDepartamentosCommandHandler>
{ ... }
```

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.API/Controllers/BaseCrudController.cs` | Implementação do controller genérico |
| `OrionERP.API/Extensions/HttpResponseExtensions.cs` | Método `AddPaginationHeaders()` |
| `OrionERP.Application/Common/Interfaces/ICommand.cs` | Interfaces `ICreateCommand`, `IUpdateCommand`, `IDeleteCommand` |
| `OrionERP.Application/Common/Interfaces/IQuery.cs` | Interfaces `IGetByIdQuery`, `IGetAllQuery` |
| `OrionERP.Infrastructure/Services/QueryStringParserServices.cs` | Parseia query string → lista de `QueryFilter` |

## Integrações

- Usa `IMediator` para despachar Commands e Queries
- Usa `IQueryStringParserService` para transformar query string em filtros
- Integra com [[CQRS e MediatR]] via `_mediator.Send()`
- Integra com [[Autenticação JWT]] via `[Authorize]`

## Configuração

Controllers concretos só precisam:
1. Herdar de `BaseCrudController<...>` com os tipos corretos
2. Definir a rota com `[Route("api/[controller]")]`
3. Passar os parâmetros necessários ao construtor base

## Observações importantes

- **Contrato estável** — não modificar sem justificativa arquitetural explícita
- Controllers nunca contêm lógica de negócio
- O `GetById` retorna o ID via reflection no response — o `TResponse` precisa ter propriedade `Id`
- O `AuthController` é uma exceção que não herda de `BaseCrudController` por ter operações distintas
