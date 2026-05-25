---
tags: [orionerp, almoxarifado, crud, empresa-scoped, estoque]
relacionado: 
    - "[[Módulo Empresa]]"
    - "[[Módulo Tipo de Almoxarifado]]"
    - "[[Módulo Departamento]]"
    - "[[Módulo Centro de Custo]]"
    - "[[Módulo PDV]]"
    - "[[BaseCrudController]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Almoxarifado

Representa um local de armazenamento de materiais dentro de uma Empresa. Está associado obrigatoriamente a um `TipoAlmoxarifado` e opcionalmente a um `Departamento` e `CentroCusto`. É pré-requisito para o módulo de `Pdv`.

## Como funciona

**Entidade `Almoxarifado`:**
- EmpresaScoped — filtro global por `EmpresaId`
- Implementa `ITenantAware` (tem `TenantId`)
- Campos: `TenantId`, `EmpresaId`, `TipoAlmoxarifadoId` (obrigatório), `DepartamentoId` (opcional), `CentroCustoId` (opcional), `Codigo`, `Nome`, `Endereco`, `Ativo`, `ExternalId`
- Relacionamentos: pertence a `Empresa`, `TipoAlmoxarifado`, `Departamento?`, `CentroCusto?`; tem muitos `Pdvs`

**CRUD completo via `BaseCrudController`:**

| Endpoint | Rota |
|---|---|
| Criar | `POST /api/almoxarifados` |
| Buscar | `GET /api/almoxarifados/{id}` |
| Listar | `GET /api/almoxarifados?Ativo.eq=true&pageNumber=1` |
| Atualizar | `PUT /api/almoxarifados/{id}` |
| Deletar | `DELETE /api/almoxarifados/{id}` |

**Validação (CreateAlmoxarifadosValidator):**
- `TipoAlmoxarifadoId` > 0 (obrigatório)
- `Codigo` não vazio
- `Nome` não vazio

## Arquivos principais

| Arquivo                                                                                     | Responsabilidade       |
| ------------------------------------------------------------------------------------------- | ---------------------- |
| `OrionERP.Domain/Entities/Almoxarifado.cs`                                                  | Entidade de domínio    |
| `OrionERP.Infrastructure/Persistence/Configurations/AlmoxarifadoConfiguration.cs`           | Configuração EF        |
| `OrionERP.API/Controllers/AlmoxarifadosController.cs`                                       | Controller HTTP        |
| `OrionERP.Application/Features/Almoxarifados/Commands/CreateAlmoxarifadosCommandHandler.cs` | Handler de criação     |
| `OrionERP.Application/Features/Almoxarifados/Commands/CreateAlmoxarifadosValidator.cs`      | Validação              |
| `OrionERP.Application/Features/Almoxarifados/Commands/UpdateAlmoxarifadosCommandHandler.cs` | Handler de atualização |
| `OrionERP.Application/Features/Almoxarifados/Commands/DeleteAlmoxarifadosCommandHandler.cs` | Handler de exclusão    |
| `OrionERP.Application/Features/Almoxarifados/Queries/`                                      | GetById e GetAll       |
| `OrionERP.Application/Features/Almoxarifados/Models/`                                       | DTOs e modelos         |

## Integrações

- [[Módulos/Multi-Tenancy]] — `EmpresaId` auto-populado pelo `SaveChangesAsync`
- [[Módulo Empresa]] — FK obrigatória para `EmpresaId`
- [[Módulo Tipo de Almoxarifado]] — FK obrigatória para `TipoAlmoxarifadoId`
- [[Módulo Departamento]] — FK opcional para `DepartamentoId`
- [[Módulo Centro de Custo]] — FK opcional para `CentroCustoId`
- [[Módulo PDV]] — Pdv referencia um `AlmoxarifadoId`

## Configuração

Nenhuma configuração especial.

## Observações importantes

- Invariante: Almoxarifado deve ter sempre um `TipoAlmoxarifado` válido
- `DepartamentoId` e `CentroCustoId` são opcionais
- `ExternalId` reservado para migração do legado Delphi
