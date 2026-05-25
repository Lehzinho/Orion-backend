---
tags:
  - orionerp
  - grupo-preparo
  - crud
  - empresa-scoped
  - restaurante
  - producao
relacionado:
  - "[[Módulo Empresa]]"
  - "[[Módulos/Multi-Tenancy]]"
  - "[[BaseCrudController]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Grupo de Preparo

Representa um agrupamento de itens por estação de preparo dentro de uma Empresa. Categoriza produtos/itens do cardápio por área de produção (ex: Cozinha Fria, Grelhados, Bar, Bebidas). Usado no fluxo de produção do restaurante.

## Como funciona

**Entidade `GrupoPreparo`:**
- EmpresaScoped — `EmpresaId` obrigatório
- Implementa `ITenantAware` (tem `TenantId`)
- Campos: `TenantId`, `EmpresaId`, `Nome`, `Descricao`
- Sem relacionamentos explícitos definidos na entidade atualmente

**CRUD completo via `BaseCrudController`:**

| Endpoint | Rota |
|---|---|
| Criar | `POST /api/gruposPreparos` |
| Buscar | `GET /api/gruposPreparos/{id}` |
| Listar | `GET /api/gruposPreparos?Nome.contains=...&pageNumber=1` |
| Atualizar | `PUT /api/gruposPreparos/{id}` |
| Deletar | `DELETE /api/gruposPreparos/{id}` |

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/GruposPreparos.cs` | Entidade de domínio (`GrupoPreparo`) |
| `OrionERP.Infrastructure/Persistence/Configurations/GruposPreparos.cs` | Configuração EF |
| `OrionERP.API/Controllers/GruposPreparosController.cs` | Controller HTTP |
| `OrionERP.Application/Features/GruposPreparos/Commands/CreateGruposPreparosCommandHandler.cs` | Handler de criação |
| `OrionERP.Application/Features/GruposPreparos/Commands/CreateGruposPreparosValidator.cs` | Validação |
| `OrionERP.Application/Features/GruposPreparos/Commands/UpdateGruposPreparosCommandHandler.cs` | Handler de atualização |
| `OrionERP.Application/Features/GruposPreparos/Commands/DeleteGruposPreparosCommandHandler.cs` | Handler de exclusão |
| `OrionERP.Application/Features/GruposPreparos/Queries/` | GetById e GetAll |
| `OrionERP.Application/Features/GruposPreparos/Models/` | DTOs e modelos |

## Integrações

- [[Módulos/Multi-Tenancy]] — `EmpresaId` e `TenantId` auto-populados pelo `SaveChangesAsync`
- [[Módulo Empresa]] — `EmpresaId` obrigatório

## Configuração

Nenhuma configuração especial. Segue padrão CRUD do `BaseCrudController`.

## Observações importantes

- Módulo novo — **não estava documentado no vault anteriormente**
- Pertence ao domínio de produção do restaurante (Fase 2 — Módulos de Negócio)
- A relação com produtos/itens do cardápio será definida quando o módulo de Produtos for implementado
- Verificar se Global Query Filter por `EmpresaId` está configurado no `OrionDbContext` para esta entidade
