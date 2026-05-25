
# 🎯 Objetivo

Definir o modelo unificado de controle de:

- Movimento (diário por empresa)
    
- Turnos de operadores
    
- Lançamentos de caixa
    
- Formas de pagamento compatíveis com NF-e
    

---

## 🧱 Entidades principais

### Movimento

Representa o dia operacional da empresa.

- 1 movimento aberto por empresa
    
- Controla o fechamento global
    

---

### Turno

Representa a sessão de trabalho do operador.

- Vinculado ao movimento
    
- Vinculado ao PDV
    

---

### CaixaLancamento

Representa uma operação financeira.

Exemplos:

- Venda
    
- Crédito em cartão interno
    
- Sangria
    

---

### CaixaPagamento

Representa a forma de pagamento utilizada.

- Baseado no padrão NF-e
    
- Pode possuir especializações
    

---

## 💳 Formas de pagamento (NF-e)

|Código|Descrição|
|---|---|
|01|Dinheiro|
|03|Cartão Crédito|
|04|Cartão Débito|
|05|Crédito Loja (Cartão Interno)|
|17|PIX|
|99|Outros|

---

## 🔗 Especializações de pagamento

- Cartão (TEF)
    
- Cartão Interno
    
- Conta de Hóspede