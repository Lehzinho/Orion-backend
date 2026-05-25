---
tags: [orionerp, serilog, logging, observabilidade, infraestrutura]
relacionado: 
    - "[[Arquitetura Geral]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# Logging e Observabilidade

Logging estruturado via Serilog configurado para console e arquivo. Enriquece automaticamente todos os logs com contexto da aplicação.

## Como funciona

Serilog é configurado no `IHostBuilder` via `UseCustomLogging()` antes do build da aplicação.

**Configuração do pipeline Serilog:**
- Lê configuração do `appsettings.json` (sinks, níveis, etc.)
- Lê serviços do DI para enriquecimento dinâmico
- Enriquece com `LogContext` (permite `LogContext.PushProperty(...)` em qualquer ponto)
- Adiciona propriedade `Application = "OrionERP"` em todos os logs

**O que é logado por padrão:**
- `TenantMiddleware` — resolução de tenant por slug
- `BaseCrudController` — cada operação CRUD com tipo e ID da entidade
- `LoginCommandHandler` — tentativas de login (sucesso e falha)
- `ValidationBehavior` — falhas de validação por tipo de request
- `Repository<T>` — erros de filtro dinâmico e ordenação
- `ExceptionHandlingMiddleware` — exceções não tratadas com stack trace

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Infrastructure/Configurations/LoggingConfiguration.cs` | Extensão `UseCustomLogging()` para o `IHostBuilder` |
| `OrionERP.API/Program.cs` | `builder.Host.UseCustomLogging()` — ponto de registro |
| `appsettings.json` / `appsettings.Development.json` | Configuração de sinks e níveis via JSON |

## Integrações

- Todos os serviços injetam `ILogger<T>` via DI — logs estruturados com tipo

## Configuração

Exemplo de `appsettings.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/orionerp-.log", "rollingInterval": "Day" } }
    ]
  }
}
```

## Observações importantes

- Logs de desenvolvimento incluem dados sensíveis via `EnableSensitiveDataLogging()` — **não ativar em produção**
- O `DatabaseConfiguration` registra queries SQL no console em desenvolvimento via `LogTo`
- Em produção, remover `EnableSensitiveDataLogging` e revisar os sinks
