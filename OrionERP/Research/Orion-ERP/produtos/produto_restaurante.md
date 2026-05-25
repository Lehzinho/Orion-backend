---
title: Domínio de Produtos — ERP + PDV para Restaurante
type: domain-spec
project: OrionERP
tags: [orionerp, produtos, cardapio, pdv, restaurante, estoque, variantes]
updated: 2026-03-24
---

← [[Orion-ERP|🏗️ Orion-ERP — Arquitetura]] | [[README|🏠 Projeto]]

---

# Especificação do Domínio - ERP + PDV para Restaurante



## Princípios Gerais do Modelo

1. Todo produto vendável **sempre** possui pelo menos 1 registro em `produto_variants`  
   → Mesmo itens simples (cerveja lata, água) têm variant "Padrão" ou "Único"

2. Preço **nunca** fica em `produtos` nem diretamente em `produto_variants`  
   → Sempre em `precos_variants` (tabela_preco + variant)

3. Receitas/composição (`produtos_receitas`) ficam no **sabor** (tipo FLAVOR) ou na variant  
   → Nunca no produto container (ex: Pizza)

4. Customização complexa (sabores mistos, adicionais) usa **modifier groups**  
   → Sabores fixos simples (suco laranja, morango) → viram variants (Opção A)

5. Cardápios podem herdar uns dos outros (`parent_id`)  
   → Facilita "Cardápio Noturno herda do Padrão e adiciona pizzas"

6. Multi-tenant rigoroso: `tenant_id` obrigatório em quase todas as tabelas

7. Auditoria padronizada: created_at, updated_at, created_by, updated_by, ativo

8. Tipos de produto (enum string):
   - PRODUCT   → item principal vendável
   - FLAVOR    → sabor usado em fração ou como variant fixa
   - TOPPING   → adicional (borda, bacon, queijo extra…)
   - INGREDIENT → insumo de estoque (não aparece no cardápio)
   - COMBO     → produto combo
   - SERVICE   → serviços (ex: taxa de entrega, couvert)

## Domínios e Tabelas

### 1. Tenant / Empresa / Usuários

- tenants
- empresas
- usuarios

Campos obrigatórios em tabelas principais:  
tenant_id, created_at, updated_at, created_by, updated_by, ativo

### 2. Unidades de Medida & Categorias

- unidades_medida  
  - sigla (un, g, ml, kg, litro, porção, cx)  
  - nome  
  - tipo (UNIDADE, MASSA, VOLUME, EMBALAGEM)

- categorias  
  - tenant_id  
  - nome  
  - ordem  
  - parent_id (hierarquia: Bebidas → Refrigerantes)

### 3. Produtos & Variantes

- produtos  
  - tenant_id  
  - categoria_id  
  - nome  
  - tipo (PRODUCT, FLAVOR, TOPPING, INGREDIENT, COMBO, SERVICE)  
  - descricao  
  - sku (único)  
  - controla_estoque  
  - tempo_preparo_min  
  - ativo

- produto_variants  
  - produto_id  
  - nome ("Grande", "500 ml", "Padrão", "Laranja 500 ml")  
  - partes (1 = não fracionável)  
  - sku  
  - ordem  
  - ativo

Regra: todo produto vendável tem **pelo menos 1 variant**

### 4. Nutrição & Alergênicos

- produto_nutricao  
  - produto_id  
  - porcao_gramas  
  - calorias, proteinas, carboidratos, gorduras_totais, sodio, fibras, acucares

- alergenicos  
  - nome (GLUTEN, LEITE, CASTANHA, CAMARAO, SOJA, OVO, PEIXE…)

- produto_alergenicos (N:N)

### 5. Ingredientes & Receitas

- ingredientes  
  - tenant_id  
  - nome  
  - unidade_padrao_id

- produtos_receitas  
  - produto_id OU variante_id  
  - ingrediente_id  
  - quantidade  
  - unidade_id  
  → Receita sempre ligada ao sabor/variant (nunca ao container)

### 6. Modificadores (customização)

- modifier_groups  
  - tenant_id  
  - nome ("Sabores", "Bordas", "Toppings", "Base Açaí")  
  - min_selecoes, max_selecoes, max_partes, obrigatorio, ordem

- modifier_options  
  - grupo_id  
  - produto_id (opcional – se ligado a sabor)  
  - nome  
  - preco_adicional  
  - ordem  
  - ativo

- produto_modifier_groups (N:N)  
  → quais grupos estão disponíveis para cada produto

Regra importante:  
- Frações → campo `partes` > 0  
- Adicionais/extras → `partes` = NULL

### 7. Preços

- tabelas_preco  
  - tenant_id  
  - nome ("Padrão", "Happy Hour", "Delivery", "Noturno")

- precos_variants  
  - tabela_preco_id  
  - produto_variant_id  
  - preco  
  - preco_promocional  
  - data_inicio, data_fim  
  → UNIQUE (tabela_preco_id, produto_variant_id)

### 8. Cardápios

- cardapios  
  - tenant_id  
  - nome  
  - tabela_preco_id  
  - parent_id (herança)

- cardapio_categorias  
  - cardapio_id  
  - categoria_id  
  - ordem

- cardapio_itens  
  - cardapio_id  
  - produto_id  
  - categoria_id  
  - ordem  
  - ativo

### 9. PDV & Ativação por horário

- pdvs  
  - tenant_id  
  - empresa_id  
  - nome ("Balcão 1", "Bar", "Delivery")

- pdv_cardapios  
  - pdv_id  
  - cardapio_id  
  - hora_inicio, hora_fim  
  - prioridade (maior vence)  
  - dias_semana (ex: 'SEG,TER,QUA,QUI,SEX')

### 10. Combos

- combos  
  - produto_id (tipo COMBO)  
  - nome  
  - preco_fixo (opcional)

- combo_itens  
  - combo_id  
  - produto_id  
  - quantidade_min, quantidade_max  
  - obrigatorio  
  - ordem

### 11. Vendas (resumo)

- vendas  
  - tenant_id  
  - pdv_id  
  - data  
  - status  
  - valor_total  
  - created_by

- venda_itens  
  - venda_id  
  - produto_variant_id  
  - quantidade  
  - preco_unitario  
  - observacao

- venda_item_modifiers  
  - venda_item_id  
  - modifier_option_id  
  - partes (NULL = adicional)  
  - quantidade  
  - preco_adicional

### 12. Estoque

- estoque_variants  
  - produto_variant_id  
  - tenant_id  
  - quantidade_atual  
  - quantidade_min  
  - localizacao ("Cozinha", "Bar", "Estoque Central")  
  - updated_at

## Regras de Negócio Importantes para Implementação

1. Todo produto vendável → obrigatoriamente tem ≥ 1 produto_variant
2. Preço sempre consultado via precos_variants (nunca de produto ou variant)
3. Sabores fixos simples (suco laranja, morango) → variants do produto "Suco"
4. Sabores misturáveis (pizza, açaí casadinho) → modifier_group "Sabores"
5. Baixa de estoque:  
   - Simples → -quantidade da variant  
   - Fracionado → proporcional pelas partes + receitas dos sabores
6. Cardápio ativo no PDV:  
   SELECT … FROM pdv_cardapios WHERE hora_atual BETWEEN inicio AND fim ORDER BY prioridade LIMIT 1
7. Herança de cardápio: se não encontrar item no cardápio atual → busca no parent_id recursivamente
8. Combos não podem conter outros combos (evitar recursão infinita)

## Fluxo Típico de Venda (Pizza + Happy Hour)

1. PDV consulta pdv_cardapios → determina cardápio ativo (Happy Hour)
2. Cardápio → tabela_preco = "Happy Hour"
3. Lista produtos/variants disponíveis via cardapio_itens + precos_variants
4. Cliente pede: Pizza Grande ½ Calabresa + ½ Portuguesa + Borda Cheddar
5. venda_itens → Pizza Grande (variant)
6. venda_item_modifiers → 2 linhas Calabresa (partes=2), Portuguesa (partes=2), Cheddar (partes=NULL)
7. Calcula preço: base da variant + adicionais
8. Baixa estoque: receitas de Calabresa e Portuguesa × 0.5 cada

Pronto para gerar entidades EF Core, validações, services, etc.

---

## 🔗 Ver Também

- [[movimento_turno_caixa|💰 Módulo Movimento / Turno / Caixa]] — as vendas alimentam os lançamentos de caixa
- [[movimento_caixa_fluxos|🔄 Fluxo de Venda → Lançamento de Caixa]]
- [[Orion-ERP|🏗️ Arquitetura Geral]]
- [[README|🏠 Projeto OrionERP]]