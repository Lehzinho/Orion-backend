---
tags:
  - orionerp
  - centro-custo
  - crud
  - empresa-scoped
  - financeiro
relacionado:
  - "[[Módulo Empresa]]"
  - "[[Módulo Almoxarifado]]"
  - "[[Módulos/Multi-Tenancy]]"
  - "[[BaseCrudController]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Centro de Custo

Representa um centro de custo para agrupamento financeiro dentro de uma Empresa. Usado para categorizar despesas e receitas operacionais. Pode ser referenciado por Almoxarifados.

## Como funciona

**Entidade `CentroCusto`:**
- EmpresaScoped — filtro global por `EmpresaId`
- Implementa `ITenantAware` (tem `TenantId`)
- Campos: `TenantId`, `EmpresaId`, `Codigo`, `Nome`, `Ativo`, `ExternalId`
- Relacionamentos: pode ter muitos `Almoxarifados`

**CRUD completo via `BaseCrudController`:**

| Endpoint | Rota |
|---|---|
| Criar | `POST /api/centroscustos` |
| Buscar | `GET /api/centroscustos/{id}` |
| Listar | `GET /api/centroscustos?Nome.contains=...&pageNumber=1` |
| Atualizar | `PUT /api/centroscustos/{id}` |
| Deletar | `DELETE /api/centroscustos/{id}` |

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/CentroCusto.cs` | Entidade de domínio |
| `OrionERP.Infrastructure/Persistence/Configurations/CentroCustoConfiguration.cs` | Configuração EF |
| `OrionERP.API/Controllers/CentrosCustosController.cs` | Controller HTTP |
| `OrionERP.Application/Features/CentrosCustos/Commands/CreateCentrosCustosCommandHandler.cs` | Handler de criação |
| `OrionERP.Application/Features/CentrosCustos/Commands/CreateCentrosCustosValidator.cs` | Validação |
| `OrionERP.Application/Features/CentrosCustos/Commands/UpdateCentrosCustosCommandHandler.cs` | Handler de atualização |
| `OrionERP.Application/Features/CentrosCustos/Commands/DeleteCentrosCustosCommandHandler.cs` | Handler de exclusão |
| `OrionERP.Application/Features/CentrosCustos/Queries/` | GetById e GetAll handlers |
| `OrionERP.Application/Features/CentrosCustos/Models/` | DTOs e modelos |

## Integrações

- [[Módulos/Multi-Tenancy]] — `EmpresaId` auto-populado pelo `SaveChangesAsync`
- [[Módulo Empresa]] — FK obrigatória para `EmpresaId`
- [[Módulo Almoxarifado]] — Almoxarifado pode referenciar um `CentroCustoId` (opcional)

## Configuração

Nenhuma configuração especial — segue padrão CRUD do `BaseCrudController`.

## Observações importantes

- Invariante: CentroCusto sempre pertence a uma Empresa
- `Codigo` é campo de identificação no legado Delphi — manter consistência na migração
- `ExternalId` reservado para integração com sistema legado
