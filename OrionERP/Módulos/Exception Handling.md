---
tags: [orionerp, excecoes, middleware, erros, validacao]
relacionado: 
    - "[[Arquitetura Geral]]" 
    - "[[CQRS e MediatR]]"
    - "[[ValidationBehavior]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# Exception Handling

Middleware centralizado que intercepta exceções não tratadas e as mapeia para respostas HTTP padronizadas. Elimina try/catch nos controllers e handlers para erros previsíveis.

## Como funciona

`ExceptionHandlingMiddleware` envolve toda a pipeline HTTP. Qualquer exceção não capturada chega aqui e é convertida em `ErrorResponse` JSON.

**Mapeamento de exceções:**

| Exceção | HTTP Status | Quando usar |
|---|---|---|
| `UnauthorizedException` | `401 Unauthorized` | Credenciais inválidas, tenant não identificado |
| `NotFoundException` | `404 Not Found` | Entidade não encontrada pelo ID |
| `ValidationException` | `400 Bad Request` | Falhas de validação FluentValidation (com detalhes por campo) |
| `BadHttpRequestException` | `400 Bad Request` | Erros de parsing de requisição |
| `InternalServerException` | `500 Internal Server Error` | Erros internos conhecidos |
| `Exception` (genérica) | `500 Internal Server Error` | Em produção: mensagem genérica. Em dev: mensagem real |

**Formato da resposta:**
```json
{
  "statusCode": 400,
  "message": "Um ou mais erros de validação ocorreram.",
  "errors": {
    "Nome": ["'Nome' não pode ser vazio."],
    "EmpresaId": ["'EmpresaId' deve ser maior que 0."]
  },
  "timestamp": "2026-03-24T10:00:00Z"
}
```

Para erros não-validação, `errors` é `null`.

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.API/Middlewares/ExceptionHandlingMiddleware.cs` | Interceptação e mapeamento de exceções |
| `OrionERP.API/Models/Responses/ErrorResponse.cs` | Modelo de resposta de erro |
| `OrionERP.Application/Common/Exceptions/ValidationException.cs` | Exceção com dicionário de erros por campo |
| `OrionERP.Application/Common/Exceptions/NotFoundException.cs` | Exceção para entidade não encontrada |
| `OrionERP.Application/Common/Exceptions/UnauthorizedException.cs` | Exceção de autenticação/autorização |
| `OrionERP.Application/Common/Exceptions/InternalServerException.cs` | Exceção de erro interno |

## Integrações

- `ValidationBehavior` lança `ValidationException` após coletar erros do FluentValidation
- Handlers lançam `NotFoundException` quando `GetByIdAsync` retorna `null`
- `LoginCommandHandler` lança `UnauthorizedException` em falhas de credenciais
- Middleware registrado antes de `UseAuthentication` via `app.RegisterMiddlewares()`

## Configuração

Registrado via `MiddelwaresExtensions.RegisterMiddlewares()`:
```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TenantMiddleware>();
```

## Observações importantes

- O middleware expõe a mensagem real da exceção apenas em desenvolvimento
- Em produção, exceções não mapeadas retornam mensagem genérica para não vazar detalhes internos
- `ValidationException` serializa os erros de validação por campo — útil para feedback de formulários
- Handlers nunca devem capturar exceções de domínio — deixar subir para o middleware
