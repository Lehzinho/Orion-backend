---
tags:
  - orionerp
  - workers
  - background-services
  - filas
  - eventos
  - async
  - multi-tenancy
relacionado:
  - "[[Módulos/Multi-Tenancy]]"
  - "[[OrionDbContext]]"
  - "[[Repository Genérico]]"
status: ativo
tipo: arquitetura
versao: 1.0.0
created: 2026-03-28
---

# Processamento Assíncrono

Guia arquitetural para implementar workers, filas, eventos e qualquer processamento fora do ciclo HTTP no OrionERP. O sistema foi projetado para suportar esses cenários — mas exige que o contexto de tenant seja inicializado explicitamente.

---

## Por que o DbContext suporta processamento background

O `TenantProvider` usa `AsyncLocal<T>` — **não depende de `HttpContext`**. O contexto de tenant flui corretamente através de `await` em qualquer tipo de execução assíncrona.

| Aspecto | Estado |
|---|---|
| `TenantProvider` usa `AsyncLocal` | ✅ Não vinculado ao HTTP |
| `ITenantProvider?` nullable no DbContext | ✅ Migrations e ferramentas EF funcionam sem injeção |
| Null/zero guards nos query filters | ✅ Sem crash quando provider sem contexto |
| `IDbContextFactory<OrionDbContext>` registrado | ✅ Workers podem criar DbContext fora de scope HTTP |
| Guard em `SaveChangesAsync` | ✅ Falha explícita se tentar gravar sem contexto inicializado |

---

## Regra fundamental

> Em qualquer worker ou background service que opere sobre entidades tenant-scoped, **chamar `SetCurrentTenantContext` é obrigatório antes de qualquer operação de banco**.

O `TenantId` e `EmpresaId` **não vêm do HTTP** nesses contextos — vêm do payload da mensagem, do job agendado, ou de alguma fonte explícita.

---

## Padrão obrigatório para workers

```csharp
public class MeuWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MeuWorker(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Payload da mensagem/job deve conter contexto de tenant
            var job = await _fila.ReceberAsync(stoppingToken);

            // 1. Criar escopo DI próprio (BackgroundService é Singleton)
            using var scope = _scopeFactory.CreateScope();

            // 2. Resolver serviços scoped
            var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<MinhaEntidade>>();

            // 3. OBRIGATÓRIO: inicializar contexto com dados do payload
            tenantProvider.SetCurrentTenantContext(
                job.TenantId,
                job.EmpresaId,
                job.TenantSlug,
                userAgent: null,   // sem HTTP, sem user agent
                ipAddress: null);  // sem HTTP, sem IP

            // 4. Operações normais — filtros e auditoria funcionam
            var entidade = await repository.GetByIdAsync(job.EntidadeId);
            // ...
            await repository.SaveChangesAsync();
        }
    }
}
```

---

## Regras de DI em background services

| Tipo de serviço | Lifetime no DI | Como usar em worker |
|---|---|---|
| `BackgroundService` / `IHostedService` | Singleton | Injeta `IServiceScopeFactory`, cria scope por job |
| `OrionDbContext` | Scoped | Não injetar diretamente — resolve via scope |
| `IRepository<T>` | Scoped | Não injetar diretamente — resolve via scope |
| `ITenantProvider` | Scoped | Não injetar diretamente — resolve via scope |
| `IDbContextFactory<OrionDbContext>` | Singleton | Pode injetar diretamente se precisar de DbContext raw |

---

## Comportamento dos query filters em workers

Quando o worker chama `SetCurrentTenantContext` corretamente:
- `CurrentTenantId != null` → filtros TenantScoped ativos
- `CurrentEmpresaId != 0` → filtros EmpresaScoped ativos
- `SaveChangesAsync` popula `TenantId`, `EmpresaId`, auditoria normalmente

Quando o worker **não** chama `SetCurrentTenantContext`:
- Leituras: filtros bypassados (`null == null` → retorna todos os registros) — **risco de leitura cross-tenant**
- Escritas em entidade `ITenantAware`: `InvalidOperationException` lançada — **proteção ativa**

---

## Payload de jobs/mensagens

Todo job que opera sobre entidades tenant-scoped deve carregar no payload:

```csharp
public record TenantJobPayload
{
    public string TenantId { get; init; } = default!;
    public long EmpresaId { get; init; }
    public string TenantSlug { get; init; } = default!;
    // ... dados específicos do job
}
```

---

## Jobs sem contexto de tenant (operações de sistema)

Jobs que operam em nível de sistema (ex: limpeza de tokens expirados, manutenção global) podem rodar **sem** `SetCurrentTenantContext`. Nesse caso:
- Filtros são bypassados — todas as entidades visíveis
- Usar apenas para operações explicitamente cross-tenant
- **Documentar** no código que é uma operação de sistema intencional

```csharp
// Exemplo: limpeza de RefreshTokens expirados (entidade Global — sem ITenantAware)
// Não requer SetCurrentTenantContext pois RefreshToken não é ITenantAware
var tokensExpirados = await context.Set<RefreshToken>()
    .Where(t => t.ExpiresAt < DateTime.UtcNow)
    .ToListAsync();
```

---

## Infraestrutura de filas (YAGNI — implementar quando necessário)

O sistema está **preparado** para receber infraestrutura de filas, mas não implementa nenhuma ainda. Quando o primeiro módulo exigir processamento assíncrono, escolher conforme o requisito:

| Cenário | Tecnologia sugerida |
|---|---|
| Fila simples em processo | `System.Threading.Channels` |
| Filas distribuídas leves | MassTransit + RabbitMQ |
| Azure-first | Azure Service Bus + MassTransit |
| Outbox pattern | MassTransit Outbox ou implementação própria |

Qualquer que seja a escolha, o padrão de bootstrapping de tenant descrito acima permanece o mesmo.

---

## Eventos de domínio (YAGNI — implementar quando necessário)

Quando módulos de domínio gerarem eventos (ex: `PedidoCriado`, `EstoqueAtualizado`), o handler do evento deve seguir o mesmo padrão de worker acima — criar scope, resolver `ITenantProvider`, inicializar contexto com dados do evento antes de qualquer operação de banco.

---

## Checklist para novo worker/handler de evento

- [ ] O worker herda `BackgroundService` ou implementa `IHostedService`?
- [ ] Injeta `IServiceScopeFactory` (não `IRepository` ou `DbContext` diretamente)?
- [ ] Cria um `scope` por job/mensagem processada?
- [ ] Chama `SetCurrentTenantContext` antes de qualquer operação de banco?
- [ ] O payload da mensagem carrega `TenantId`, `EmpresaId` e `TenantSlug`?
- [ ] Se for operação de sistema (cross-tenant), está documentado no código?
