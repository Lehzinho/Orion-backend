---
title: Domínio de Pedidos e Vendas — OrionERP
type: domain-spec
project: OrionERP
tags: [orionerp, pedido, venda, caixa, pagamento, pdv, restaurante]
updated: 2026-04-06
---

← [[Orion-ERP|🏗️ Orion-ERP — Arquitetura]] | [[README|🏠 Projeto]]

---

# Especificação do Domínio — Pedidos e Vendas

---

## Princípios Gerais do Modelo

1. **Pedido ≠ Venda** — Pedido representa intenção de consumo; Venda representa transação comercial recebida.

2. **Pedido não gera lançamento de caixa** — apenas Vendas finalizadas e recebidas geram `CaixaLancamento`.

3. **Venda agrupa Pedidos** — relação N:N via `VendaPedido`. MVP é 1:1 (balcão); Fase 2 suporta N Pedidos por Venda via Comanda.

4. **NF-e nasce da Venda, não do Pedido** — a nota fiscal é emitida sobre a transação comercial, não sobre o lançamento operacional.

5. **PontoEntrega é nullable no Pedido** — balcão não exige mesa. Fase 2 torna obrigatório para pedidos de restaurante.

6. **ComandaId é nullable no Pedido** — Fase 2 adiciona FK nullable. Pedidos de balcão ficam com `null`. Sem breaking change.

7. **precoUnitario em PedidoItem é snapshot imutável** — garante que alterações futuras de preço não afetam pedidos já lançados.

8. **Dados financeiros são imutáveis** — `CaixaLancamento` e `CaixaPagamento` não podem ser deletados ou alterados após criação.

---

## Distinção Pedido vs Venda

| Aspecto | Pedido | Venda |
|---|---|---|
| Quando nasce | Ao lançar itens no PDV | Ao receber o pagamento |
| O que representa | Intenção de consumo | Transação comercial |
| Gera ticket cozinha | ✅ (query por GrupoPreparo) | ❌ |
| Gera lançamento de caixa | ❌ | ✅ via CaixaLancamento |
| Gera Nota Fiscal | ❌ | ✅ Fase 2 |
| Status | ABERTO → CANCELADO / ENTREGUE | PENDENTE → FINALIZADA / CANCELADA |
| Vínculo financeiro | Turno (operacional) | Movimento + Turno (financeiro) |

---

## Domínios e Tabelas

### 1. Pedido

- **pedidos**
  - empresa_id (EmpresaScoped)
  - turno_id → Turno
  - comanda_id → Comanda (nullable — Fase 2)
  - ponto_entrega_id → PontoEntrega (nullable — obrigatório apenas na Fase 2 para mesas)
  - status: `ABERTO` | `CANCELADO` | `ENTREGUE`
  - observacao (nullable)

- **pedido_itens**
  - pedido_id → Pedido
  - produto_variant_id → ProdutoVariant
  - quantidade (decimal)
  - preco_unitario (decimal — **snapshot imutável** do momento do lançamento)
  - observacao (nullable)

Regra: apenas Pedidos com status `ABERTO` aceitam adição/remoção de itens.

---

### 2. Venda

- **vendas**
  - empresa_id (EmpresaScoped)
  - movimento_id → Movimento
  - turno_id → Turno
  - valor_total (decimal)
  - status: `PENDENTE` | `FINALIZADA` | `CANCELADA`
  - data_hora (DateTime)

- **venda_pedidos** *(junction N:N)*
  - venda_id → Venda
  - pedido_id → Pedido

Regra: uma Venda só é FINALIZADA após `CaixaLancamento` criado com pagamentos que fecham o `valor_total`.

---

### 3. Caixa

- **caixa_lancamentos**
  - empresa_id (EmpresaScoped)
  - movimento_id → Movimento
  - turno_id → Turno
  - tipo: `VENDA` | `SANGRIA` | `SUPRIMENTO` *(MVP usa só VENDA)*
  - valor_total (decimal)
  - data_hora (DateTime)

- **caixa_pagamentos**
  - caixa_lancamento_id → CaixaLancamento
  - tipo_pagamento: `DINHEIRO` | `CARTAO_CREDITO` | `CARTAO_DEBITO` | `PIX` | `CARTAO_INTERNO` | `OUTROS`
  - valor (decimal)

Regra: `SUM(caixa_pagamentos.valor) == caixa_lancamentos.valor_total`  
Regra: múltiplos pagamentos permitidos por lançamento (split de formas de pagamento).  
Regra: dados de caixa são **imutáveis** após criação.

---

## Enums

```
StatusPedido:   ABERTO | CANCELADO | ENTREGUE
StatusVenda:    PENDENTE | FINALIZADA | CANCELADA
TipoLancamento: VENDA | SANGRIA | SUPRIMENTO
TipoPagamento:  DINHEIRO | CARTAO_CREDITO | CARTAO_DEBITO | PIX | CARTAO_INTERNO | OUTROS
```

---

## Escopo Multi-Tenant

| Entidade | EmpresaScoped | Global | Observação |
|---|---|---|---|
| Pedido | ✅ | ❌ | |
| PedidoItem | ✅ | ❌ | Segue escopo do Pedido |
| Venda | ✅ | ❌ | |
| VendaPedido | ✅ | ❌ | Junction — segue escopo da Venda |
| CaixaLancamento | ✅ | ❌ | |
| CaixaPagamento | ✅ | ❌ | Segue escopo do CaixaLancamento |

---

## Regras de Negócio

### Pedido
- Requer Turno com status `OPEN`
- Itens só podem ser adicionados/removidos em Pedido `ABERTO`
- `precoUnitario` em `PedidoItem` é snapshot — imutável após lançamento
- Cancelamento só permitido em Pedido `ABERTO`

### Venda
- Valor total deve coincidir com soma dos `PedidoItem.precoUnitario × quantidade` dos Pedidos incluídos
- `SUM(CaixaPagamento.valor) == CaixaLancamento.valorTotal`
- Venda `FINALIZADA` não pode ser alterada
- Cancelamento de Venda não cancela automaticamente os Pedidos

### Caixa
- Todo lançamento deve estar vinculado a Turno `OPEN`
- Dados financeiros (`CaixaLancamento`, `CaixaPagamento`) são imutáveis após criação

---

## Fluxo Típico — Balcão MVP (Phase 1)

```
1. AbrirMovimento (empresa) ──── se não existir OPEN hoje
2. AbrirTurno (PDV + operador + fundo_caixa) ──── requer Movimento OPEN
3. CriarPedido ──── requer Turno OPEN
4. AdicionarItens (produto_variant + quantidade)
       ↓ precoUnitario snapshot capturado aqui
5. [ticket produção gerado por query: PedidoItens ABERTOS por GrupoPreparo]
6. FinalizarVenda (pedidoId, pagamentos[])
       ↓ cria Venda + VendaPedido
       ↓ cria CaixaLancamento (tipo=VENDA)
       ↓ cria CaixaPagamento[] (valida soma = total)
       ↓ atualiza Pedido.status = ENTREGUE
       ↓ atualiza Venda.status = FINALIZADA
7. FecharTurno ──── cria CaixaTurnoFechamento
8. FecharMovimento ──── requer todos os Turnos fechados
```

---

## O que fica para Fase 2+

| Item | Motivo |
|---|---|
| Comanda | Agrupamento de Pedidos — `Pedido.ComandaId` nullable, sem breaking change |
| PontoEntrega obrigatório | Nullable desde Phase 1 — sem breaking change |
| Sangria / Suprimento | `TipoLancamento` já tem os valores no enum |
| Cartões Internos | `TipoPagamento.CARTAO_INTERNO` já registrado no MVP |
| KDS com confirmação de preparo | Ticket de produção é query simples no MVP |

---

## 🔗 Ver Também

- [[movimento_turno_caixa|💰 Módulo Movimento / Turno / Caixa]] — Movimento, Turno e estrutura de caixa
- [[produto_restaurante|🍽️ Domínio de Produtos]] — Produtos, variantes, preços e cardápio
- [[nota_fiscal|🧾 Notas Fiscais]] — ProdutoFiscal, EmpresaFiscal, emissão NFC-e / NFS-e (Fase 2)
- [[comanda|🪑 Comanda / Mesa]] — Agrupamento de Pedidos por mesa (Fase 2)
- [[Módulo PDV]] — Ponto de venda
- [[Módulo Grupo de Preparo]] — Agrupamentos de produção (ticket cozinha)
- [[Módulo Ponto de Entrega]] — Mesas e locais de entrega
- [[Orion-ERP|🏗️ Arquitetura Geral]]
