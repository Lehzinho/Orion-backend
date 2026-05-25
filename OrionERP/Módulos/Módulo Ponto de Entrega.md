---
tags:
  - orionerp
  - ponto-entrega
  - crud
  - empresa-scoped
  - restaurante
  - entrega
relacionado:
  - "[[Módulo Empresa]]"
  - "[[Módulos/Multi-Tenancy]]"
  - "[[BaseCrudController]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Ponto de Entrega

Representa um local de entrega de pedidos dentro de uma Empresa. Pode ser uma mesa, balcão, posição de entrega, ou qualquer ponto físico onde um pedido é servido/entregue. Usado no fluxo operacional de pedidos do restaurante.

## Como funciona

**Entidade `PontoEntrega`:**
- EmpresaScoped — `EmpresaId` obrigatório
- Implementa `ITenantAware` (tem `TenantId`)
- Campos: `TenantId`, `EmpresaId`, `Nome`, `Descricao`
- Sem relacionamentos explícitos definidos na entidade atualmente

**CRUD completo via `BaseCrudController`:**

| Endpoint | Rota |
|---|---|
| Criar | `POST /api/pontosEntregas` |
| Buscar | `GET /api/pontosEntregas/{id}` |
| Listar | `GET /api/pontosEntregas?Nome.contains=...&pageNumber=1` |
| Atualizar | `PUT /api/pontosEntregas/{id}` |
| Deletar | `DELETE /api/pontosEntregas/{id}` |

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/PontosEntregas.cs` | Entidade de domínio (`PontoEntrega`) |
| `OrionERP.Infrastructure/Persistence/Configurations/PontosEntregas.cs` | Configuração EF |
| `OrionERP.API/Controllers/PontosEntregasController.cs` | Controller HTTP |
| `OrionERP.Application/Features/PontosEntregas/Commands/CreatePontosEntregasCommandHandler.cs` | Handler de criação |
| `OrionERP.Application/Features/PontosEntregas/Commands/CreatePontosEntregasValidator.cs` | Validação |
| `OrionERP.Application/Features/PontosEntregas/Commands/UpdatePontosEntregasCommandHandler.cs` | Handler de atualização |
| `OrionERP.Application/Features/PontosEntregas/Commands/DeletePontosEntregasCommandHandler.cs` | Handler de exclusão |
| `OrionERP.Application/Features/PontosEntregas/Queries/` | GetById e GetAll |
| `OrionERP.Application/Features/PontosEntregas/Models/` | DTOs e modelos |

## Integrações

- [[Módulos/Multi-Tenancy]] — `EmpresaId` e `TenantId` auto-populados pelo `SaveChangesAsync`
- [[Módulo Empresa]] — `EmpresaId` obrigatório

## Configuração

Nenhuma configuração especial. Segue padrão CRUD do `BaseCrudController`.

## Observações importantes

- Módulo novo — **não estava documentado no vault anteriormente**
- Pertence ao domínio de pedidos/operação do restaurante (Fase 2 — Módulos de Negócio)
- A relação com pedidos/comandas será definida quando o módulo de Pedidos for implementado
- Verificar se Global Query Filter por `EmpresaId` está configurado no `OrionDbContext` para esta entidade
