---
tags: [orionerp, validacao, fluentvalidation, pipeline, mediatr]
relacionado: 
    - "[[CQRS e MediatR]]" 
    - "[[Exception Handling]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# ValidationBehavior

Pipeline behavior do MediatR que executa automaticamente todos os validators FluentValidation registrados antes do handler ser invocado. Garante que nenhum Command inválido alcance o Handler.

## Como funciona

`ValidationBehavior<TRequest, TResponse>` é um `IPipelineBehavior<TRequest, TResponse>` registrado como `Transient` no DI.

**Fluxo:**
1. MediatR invoca o behavior antes do handler
2. Resolve todos os `IValidator<TRequest>` do DI
3. Executa todos em paralelo via `Task.WhenAll`
4. Agrega todas as falhas
5. Se há falhas → lança `ValidationException` com dicionário de erros por campo
6. Se não há falhas → chama `next()` (passa para o handler)

**Validators por feature:**
Cada Command que precisa de validação tem um `AbstractValidator<TCommand>` na pasta `Commands/` da feature. Registrados automaticamente via `AddValidatorsFromAssembly`.

**Exemplos de validações existentes:**
- `CreateDepartamentosValidator` — `EmpresaId > 0`, `Nome` obrigatório/máx 100
- `CreateEmpresaValidator` — `RazaoSocial` obrigatório, `CpfCnpj` formato, `Slug` obrigatório
- `LoginCommandValidator` — `Login` e `Senha` obrigatórios
- Validators de Create e Update para todos os módulos implementados

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Application/Common/Behaviors/ValidattionBehavior.cs` | Implementação do pipeline behavior |
| `OrionERP.Application/Features/*/Commands/*Validator.cs` | Validators por feature |

## Integrações

- [[Exception Handling]] — `ValidationException` é capturada pelo `ExceptionHandlingMiddleware` e convertida em `400 Bad Request` com detalhes por campo
- [[CQRS e MediatR]] — registrado como behavior no pipeline MediatR

## Configuração

Registrado em `ApplicationExtensions`:
```csharp
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

## Observações importantes

- Se um Command não tem validator registrado, o behavior passa sem validar (comportamento esperado — não há falha)
- Todos os erros de validação são agregados antes de lançar a exceção — o cliente recebe todos os problemas de uma vez
- Queries geralmente não têm validators — são commands que precisam de validação
