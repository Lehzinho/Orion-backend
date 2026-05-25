---
title: Módulo Movimento / Turno / Caixa
type: module
project: OrionERP
status: implementado
updated: 2026-04-09
tags: [orionerp, movimento, turno, caixa, pdv, cqrs]
---

← [[Orion-ERP|🏠 Voltar ao Projeto]]

---

## Visão Geral

Módulo responsável pelo ciclo de vida de abertura e fechamento do caixa diário (Movimento) e das sessões de operador no PDV (Turno).

**Status:** ✅ Implementado e em produção (main)

---

## Entidades

### Movimento
- Representa o ciclo diário de caixa de uma empresa
- Invariante: máximo 1 Movimento **Aberto** por empresa (unique index condicional)
- Factory: `Movimento.Abrir(DateOnly data, long abertoPorId)`
- Mutation: `Movimento.Fechar(long fechadoPorId)` — guard: só fecha se Aberto

### Turno
- Representa a sessão de trabalho de um operador em um PDV específico
- Invariante: máximo 1 Turno **Aberto** por `(EmpresaId, PdvId, OperadorId)` (unique index condicional)
- Factory: `Turno.Abrir(long movimentoId, long pdvId, long operadorId)`
- Mutation: `Turno.Fechar()` — guard: só fecha se Aberto

### EmpresaParametros
- Configurações operacionais por empresa, armazenadas em coluna JSONB (`parametros`)
- Campo: `HorasCarenciaMovimento: int` (default: 4h)
- Upsert idempotente — uma entrada por empresa

### AuditoriaMovimento
- Registro gerado automaticamente ao fechar um Movimento
- Criado pelo `MovimentoFechadoEventHandler` via MediatR notification
- Invariante: máximo 1 registro por Movimento (unique index)

---

## Arquitetura

### Fluxo de Fechamento (com notification)

```
FecharMovimentoCommandHandler
  → Movimento.Fechar()
  → SaveChangesAsync()
  → IMediator.Publish(MovimentoFechadoNotification)
      → MovimentoFechadoEventHandler
          → AuditoriaMovimento criada
```

**Decisão:** `MovimentoFechadoNotification` vive em **Application** (não em Domain). O Domain não depende de MediatR.

### Idempotência (AbrirMovimento e AbrirTurno)

Ambos os handlers usam o mesmo padrão:
1. `FindAsync` busca registro Aberto existente → retorna se existir
2. Tenta criar novo → `SaveChangesAsync`
3. Se `DbUpdateException` (race condition) → busca e retorna o existente

### Lógica de Carência (GetMovimentoAtual)

| Situação | Resposta |
|---|---|
| Movimento Aberto com `Data == hoje` | `Aberto` |
| Movimento Aberto com `Data < hoje` e dentro do prazo | `EmCarencia` + horas restantes |
| Movimento Aberto com `Data < hoje` e prazo expirado | `AberturaNecessaria` + MovimentoId |
| Nenhum Movimento Aberto | `AberturaNecessaria` |

---

## Endpoints

### MovimentosController (`/api/movimentos`)

| Método | Endpoint | Descrição |
|---|---|---|
| `GET` | `/atual` | Status do movimento atual com lógica de carência |
| `POST` | `/abrir` | Abre novo movimento (idempotente) |
| `POST` | `/{id}/fechar` | Fecha movimento, gera auditoria |

### TurnosController (`/api/turnos`)

| Método | Endpoint | Descrição |
|---|---|---|
| `POST` | `/abrir` | Abre turno para PDV + Operador (idempotente) |
| `POST` | `/{id}/fechar` | Fecha turno |

### EmpresaParametrosController (`/api/empresaparametros`)

| Método | Endpoint | Descrição |
|---|---|---|
| `GET` | `/` | Retorna parâmetros da empresa |
| `PUT` | `/` | Upsert dos parâmetros |

---

## Testes

18 testes unitários cobrindo:
- `UpsertEmpresaParametrosCommandHandler` — criar e atualizar
- `GetEmpresaParametrosQueryHandler` — com dados e sem dados
- `AbrirMovimentoCommandHandler` — criar novo e retornar existente
- `FecharMovimentoCommandHandler` — fechar, não encontrado, turnos abertos
- `GetMovimentoAtualQueryHandler` — aberto hoje, sem movimento, fora de carência
- `MovimentoFechadoEventHandler` — criação de auditoria
- `AbrirTurnoCommandHandler` — criar novo e retornar existente
- `FecharTurnoCommandHandler` — fechar, não encontrado, já fechado

---

## Decisões Arquiteturais

- **Domain sem MediatR:** `MovimentoFechadoNotification` é um `INotification` definido em Application. O handler é quem publica — não a entidade.
- **JSONB com snake_case:** `EmpresaParametrosData` usa `ToJson("parametros")` (EF Core 8+) para nomear a coluna corretamente.
- **Sem DomainEvents no EntityBase:** O padrão de domain events via coleção na entidade foi descartado para manter o Domain limpo e sem dependências.
