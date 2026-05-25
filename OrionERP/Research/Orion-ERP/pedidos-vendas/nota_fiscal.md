---
title: Domínio Fiscal — Notas Fiscais — OrionERP
type: domain-spec
project: OrionERP
tags: [orionerp, fiscal, nfe, nfce, nfse, nota-fiscal, tributacao, sefaz]
updated: 2026-04-06
status: fase-2
---

← [[Orion-ERP|🏗️ Orion-ERP — Arquitetura]] | [[README|🏠 Projeto]]

---

# Especificação do Domínio — Notas Fiscais

> ⚠️ **Fase 2** — Entidades `ProdutoFiscal` e `EmpresaFiscal` são criadas como **shells** na Fase 1 (campos nullable). A emissão de NF-e e todas as demais entidades desta spec são implementadas na Fase 2+.

---

## Princípios Gerais do Modelo

1. **NF-e nasce da Venda, não do Pedido** — a nota fiscal é emitida sobre a transação comercial.

2. **Uma Venda pode gerar múltiplas notas** — NFC-e para produtos (`TipoProduto = PRODUCT`) e NFS-e para serviços (`TipoProduto = SERVICE`) emitidas separadamente quando presentes na mesma Venda.

3. **Múltiplas Vendas podem compor uma NF-e** — agrupamento de vendas em nota única (ex: faturamento de conta corrente).

4. **ProdutoFiscal isolado de Produto** — dados fiscais (NCM, CFOP, CST) ficam em entidade separada para suportar mudanças de legislação estadual sem afetar o modelo comercial.

5. **EmpresaFiscal isolado de Empresa** — dados fiscais da empresa emissora (regime tributário, série, inscrição estadual) ficam separados.

6. **NotaFiscalItem é snapshot completo** — independente de alterações futuras em `Produto` ou `ProdutoFiscal`.

7. **Emissão requer completude fiscal** — `ProdutoFiscal` e `EmpresaFiscal` devem estar completamente preenchidos antes de habilitar emissão.

---

## Domínios e Tabelas

### 1. ProdutoFiscal — Shell (Phase 1)

> Criada automaticamente junto com o `Produto` (campos nullable). Isolada de `Produto` para suportar mudanças de legislação estadual sem afetar modelo comercial.

- **produto_fiscal** *(1:1 com Produto)*
  - produto_id → Produto (unique)
  - ncm (string 8 dígitos, nullable)
  - cest (nullable)
  - cfop (nullable)
  - cst_csosn (nullable)
  - origem: `0`=nacional | `1`=estrangeira (nullable)
  - codigo_servico_lc116 (nullable — NFS-e quando TipoProduto=SERVICE)
  - aliquota_iss (decimal nullable — NFS-e)

---

### 2. EmpresaFiscal — Shell (Phase 1)

> Criada automaticamente junto com a `Empresa`. Configurada pelo operador antes de habilitar emissão.

- **empresa_fiscal** *(1:1 com Empresa)*
  - empresa_id → Empresa (unique)
  - regime_tributario: `SIMPLES_NACIONAL` | `LUCRO_PRESUMIDO` | `LUCRO_REAL` (nullable)
  - inscricao_estadual (nullable)
  - inscricao_municipal (nullable)
  - serie_nfce (int nullable)
  - numero_nfce_atual (int nullable)
  - serie_nfse (int nullable)
  - numero_nfse_atual (int nullable)
  - certificado_path (nullable)
  - ambiente: `HOMOLOGACAO` | `PRODUCAO` (nullable)

---

### 3. NotaFiscal — Fase 2

- **notas_fiscais**
  - empresa_id (EmpresaScoped)
  - tipo: `NFCE` | `NFSE`
  - numero (int)
  - serie (int)
  - chave_acesso (string 44 chars)
  - status: `PENDENTE` | `AUTORIZADA` | `CANCELADA` | `DENEGADA`
  - data_emissao (DateTime nullable)
  - valor_total (decimal)
  - xml (text nullable — preenchido após autorização SEFAZ)

- **venda_notas_fiscais** *(junction N:N)*
  - venda_id → Venda
  - nota_fiscal_id → NotaFiscal

- **nota_fiscal_itens**
  - nota_fiscal_id → NotaFiscal
  - pedido_item_id → PedidoItem *(referência de origem)*
  - descricao (snapshot)
  - quantidade, valor_unitario, valor_total (snapshot)
  - ncm, cfop, cst_csosn, origem (snapshot fiscal no momento da emissão)
  - codigo_servico_lc116, aliquota_iss (snapshot — NFS-e)

---

### 4. RegraFiscalNCM — Fase 2+

> Tabela de regras tributárias por NCM/UF. Atualizada conforme legislação estadual.

- **regras_fiscal_ncm** *(lookup global — não EmpresaScoped)*
  - ncm
  - uf_destino
  - cst
  - cfop
  - aliquota_icms
  - aliquota_reducao_bc
  - modalidade_bc
  - motivo_desoneracao
  - vigencia_inicio, vigencia_fim

---

## Enums

```
TipoNota:        NFCE | NFSE
StatusNota:      PENDENTE | AUTORIZADA | CANCELADA | DENEGADA
RegimeTributario: SIMPLES_NACIONAL | LUCRO_PRESUMIDO | LUCRO_REAL
AmbienteFiscal:  HOMOLOGACAO | PRODUCAO
OrigemProduto:   0=nacional | 1=estrangeira
```

---

## Escopo Multi-Tenant

| Entidade | EmpresaScoped | Global | Observação |
|---|---|---|---|
| ProdutoFiscal | ✅ | ❌ | 1:1 com Produto |
| EmpresaFiscal | ✅ | ❌ | 1:1 com Empresa |
| NotaFiscal | ✅ | ❌ | Fase 2 |
| VendaNotaFiscal | ✅ | ❌ | Fase 2 — junction |
| NotaFiscalItem | ✅ | ❌ | Fase 2 |
| RegraFiscalNCM | ❌ | ✅ | Fase 2+ — lookup global |

---

## Regras de Negócio

### Shells (Phase 1)
- `ProdutoFiscal` criada automaticamente com o `Produto` (todos os campos nullable)
- `EmpresaFiscal` criada automaticamente com a `Empresa` (todos os campos nullable)
- Shells não bloqueiam nenhuma funcionalidade do MVP

### Emissão (Fase 2)
- Requer `EmpresaFiscal` completamente preenchida (regime, certificado, ambiente, série)
- Requer `ProdutoFiscal` completamente preenchida para cada item da Venda
- NFC-e → itens com `TipoProduto = PRODUCT`
- NFS-e → itens com `TipoProduto = SERVICE`
- Split automático quando uma Venda contém ambos os tipos
- `NotaFiscalItem` é snapshot completo — imutável após autorização SEFAZ

---

## Fluxo de Emissão — Fase 2

```
FinalizarVenda
  ↓
[Fase 2] EmitirNotaFiscal ← Venda
  ↓ separa itens por TipoProduto
  ├── PRODUCT → gera NFC-e
  │     ↓ assina com certificado
  │     ↓ envia para SEFAZ
  │     ↓ aguarda autorização
  │     ↓ armazena XML + chave_acesso
  │     ↓ NotaFiscal.status = AUTORIZADA
  └── SERVICE → gera NFS-e
        ↓ envia para prefeitura (código LC 116)
        ↓ NotaFiscal.status = AUTORIZADA
VendaNotaFiscal (M:N) criada para cada nota emitida
```

---

## O que fica para Fase 2+

| Item | Motivo |
|---|---|
| Integração SEFAZ / emissor fiscal | Requer `EmpresaFiscal` + `ProdutoFiscal` preenchidos |
| RegraFiscalNCM | Lookup de tributação por NCM/UF/estado |
| Cancelamento de NF-e | Prazo legal SEFAZ (até 24h após autorização) |
| Carta de Correção (CC-e) | Correção de dados não essenciais |
| DANFE / impressão | Geração de PDF do documento fiscal |

---

## 🔗 Ver Também

- [[pedido_venda|🛒 Pedidos e Vendas]] — Venda é a origem das notas fiscais
- [[produto_restaurante|🍽️ Domínio de Produtos]] — TipoProduto define NFC-e vs NFS-e
- [[Orion-ERP|🏗️ Arquitetura Geral]]
