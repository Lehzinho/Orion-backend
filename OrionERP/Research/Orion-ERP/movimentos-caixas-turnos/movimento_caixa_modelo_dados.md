
## 🧩 Estrutura das tabelas

### movimentos

- id
    
- empresa_id
    
- data
    
- status (OPEN, CLOSED)
    
- opened_at
    
- closed_at
    

---

### turnos

- id
    
- movimento_id
    
- operador_id
    
- pdv_id
    
- status
    
- opened_at
    
- closed_at
    

---

### caixa_lancamentos

- id
    
- movimento_id
    
- turno_id
    
- tipo
    
- valor_total
    
- data_hora
    

---

### caixa_pagamentos

- id
    
- caixa_lancamento_id
    
- forma_pagamento
    
- valor
    
- pagamento_cartao_id
    
- pagamento_cartao_interno_id
    
- pagamento_hospede_id
    

---

### caixa_pagamento_cartao

- id
    
- nsu
    
- authorization
    
- card_holder
    
- bandeira
    
- tipo
    
- parcelas
    
- via_cliente
    
- via_estabelecimento
    

---

### caixa_pagamento_cartao_interno

- id
    
- conta_id
    
- numero_cartao
    
- tipo_cartao
    
- categoria
    

---

### caixa_pagamento_hospede

- id
    
- global_id
    
- folio_id
    
- numero_cartao
    
- room
    
- nome_hospede
    

---

### caixa_turno_fechamento

- id
    
- turno_id
    
- valor_sistema
    
- valor_informado
    
- diferenca
    
- fechado_em