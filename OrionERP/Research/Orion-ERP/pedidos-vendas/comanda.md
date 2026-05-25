---
title: Domínio de Comandas — OrionERP
type: domain-spec
project: OrionERP
tags: [orionerp, comanda, mesa, restaurante, pedido, fase2]
updated: 2026-04-06
status: fase-2
---

← [[Orion-ERP|🏗️ Orion-ERP — Arquitetura]] | [[README|🏠 Projeto]]

---

# Especificação do Domínio — Comanda / Mesa

> ⚠️ **Fase 2** — Não implementado no MVP. A adição de Comanda é **aditiva** — sem breaking change nas entidades existentes. Pedidos de balcão (sem comanda) permanecem funcionando com `comanda_id = NULL`.

---

## Princípios Gerais do Modelo

1. **Comanda agrupa Pedidos de uma mesa** — N Pedidos lançados ao longo do serviço compõem uma única Comanda.

2. **ComandaId é nullable no Pedido desde o MVP** — `pedidos.comanda_id` FK nullable já reservada. Pedidos de balcão ficam com `null`. Sem breaking change na migration.

3. **PontoEntrega obrigatório na Fase 2** — nullable desde o MVP (Phase 1). Na Fase 2, validação torna obrigatório para pedidos vinculados a Comanda.

4. **Venda fecha a Comanda** — ao finalizar a venda, N Pedidos da Comanda formam 1 Venda via `VendaPedido`.

5. **Comanda não gera lançamento de caixa** — apenas a Venda gerada ao fechar a Comanda produz `CaixaLancamento`.

---

## Domínios e Tabelas

### 1. Comanda

- **comandas**
  - empresa_id (EmpresaScoped)
  - ponto_entrega_id → PontoEntrega *(mesa, área, local)*
  - turno_id → Turno
  - status: `ABERTA` | `FECHADA`
  - aberta_em (DateTime)
  - fechada_em (DateTime nullable)

---

### 2. Alteração em Pedido (Fase 2 — migration aditiva)

```sql
-- Migration Fase 2: adiciona FK nullable em pedidos
ALTER TABLE pedidos ADD COLUMN comanda_id UUID REFERENCES comandas(id);
```

- **pedidos.comanda_id** → Comanda (nullable)
  - `NULL` → pedido de balcão (MVP, sem comanda)
  - Preenchido → pedido vinculado a mesa (Fase 2)

Regra: migration é **backwards-compatible** — pedidos existentes de balcão mantêm `comanda_id = NULL` sem qualquer alteração de comportamento.

---

## Enums

```
StatusComanda: ABERTA | FECHADA
```

---

## Escopo Multi-Tenant

| Entidade | EmpresaScoped | Global | Observação |
|---|---|---|---|
| Comanda | ✅ | ❌ | Fase 2 |

---

## Regras de Negócio

- Comanda requer Turno com status `OPEN`
- Comanda requer PontoEntrega válido (mesa)
- Pedidos vinculados à Comanda devem pertencer ao mesmo Turno
- Fechar Comanda não finaliza a Venda — o operador deve chamar `FinalizarVenda` separadamente
- Cancelamento de Comanda deve validar se há Pedidos `ABERTOS` vinculados
- `Comanda.fechada_em` preenchido apenas quando `status = FECHADA`

---

## Fluxo Típico — Restaurante (Phase 2)

```
1. Movimento + Turno abertos
2. CriarComanda (PontoEntrega = mesa X)
3. CriarPedido (comanda_id preenchido) → AdicionarItens
4. [cliente pede mais] → novo CriarPedido na mesma Comanda
5. [ao longo do serviço: N Pedidos por Comanda]
6. FecharComanda → status = FECHADA
7. FinalizarVenda (N Pedidos → 1 Venda via VendaPedido)
       ↓ CaixaLancamento + CaixaPagamento(s)
       ↓ Pedidos.status = ENTREGUE
       ↓ Venda.status = FINALIZADA
8. [Fase 2+] EmitirNotaFiscal ← Venda
```

---

## Comparação MVP vs Fase 2

| Aspecto | MVP (Balcão) | Fase 2 (Restaurante) |
|---|---|---|
| `pedidos.comanda_id` | NULL | Preenchido |
| `pedidos.ponto_entrega_id` | NULL (nullable) | Obrigatório |
| Pedidos por Venda | 1 | N |
| Fluxo de fechamento | Direto no PDV | Via Comanda → Venda |
| Breaking change | ❌ | ❌ (aditivo) |

---

## 🔗 Ver Também

- [[pedido_venda|🛒 Pedidos e Vendas]] — Comanda agrupa Pedidos que geram uma Venda
- [[Módulo Ponto de Entrega]] — Mesas e locais de entrega
- [[movimento_turno_caixa|💰 Módulo Movimento / Turno / Caixa]] — Turno é contexto obrigatório
- [[Orion-ERP|🏗️ Arquitetura Geral]]
