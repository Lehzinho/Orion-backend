---
tags:
  - orionerp
  - departamento
  - crud
  - empresa-scoped
  - dominio
relacionado:
  - "[[Módulo Empresa]]"
  - "[[Módulo Almoxarifado]]"
  - "[[Módulos/Multi-Tenancy]]"
  - "[[BaseCrudController]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Departamento

Representa uma divisão organizacional dentro de uma Empresa. É o módulo de referência para o padrão de implementação de features no OrionERP — todos os outros CRUDs seguem sua estrutura.

## Como funciona

**Entidade `Departamento`:**
- EmpresaScoped (não TenantScoped diretamente — o filtro é por `EmpresaId`)
- Implementa `ITenantAware` (tem `TenantId`)
- Campos: `TenantId`, `EmpresaId`, `Nome`, `Ativo`
- Relacionamentos: pode ter muitos `Almoxarifados`

**CRUD completo via `BaseCrudController`:**

| Endpoint | Rota |
|---|---|
| Criar | `POST /api/departamentos` |
| Buscar | `GET /api/departamentos/{id}` |
| Listar | `GET /api/departamentos?Nome.contains=TI&pageNumber=1` |
| Atualizar | `PUT /api/departamentos/{id}` |
| Deletar | `DELETE /api/departamentos/{id}` |

**Validação (CreateDepartamentosValidator):**
- `EmpresaId` > 0 (obrigatório)
- `Nome` não vazio, máximo 100 caracteres

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/Departamento.cs` | Entidade de domínio |
| `OrionERP.Infrastructure/Persistence/Configurations/DepartamentoConfiguration.cs` | Configuração EF |
| `OrionERP.API/Controllers/DepartamentosController.cs` | Controller HTTP |
| `OrionERP.Application/Features/Departamentos/Commands/CreateDepartamentosCommandHandler.cs` | Handler de criação |
| `OrionERP.Application/Features/Departamentos/Commands/CreateDepartamentosValidator.cs` | Validação FluentValidation |
| `OrionERP.Application/Features/Departamentos/Commands/UpdateDepartamentosCommandHandler.cs` | Handler de atualização |
| `OrionERP.Application/Features/Departamentos/Commands/DeleteDepartamentosCommandHandler.cs` | Handler de exclusão |
| `OrionERP.Application/Features/Departamentos/Queries/` | GetById e GetAll handlers |
| `OrionERP.Application/Features/Departamentos/Models/` | DTOs e modelos de request/response |

## Integrações

- [[Módulos/Multi-Tenancy]] — `EmpresaId` auto-populado pelo `SaveChangesAsync` do `OrionDbContext`
- [[Módulo Empresa]] — FK obrigatória para `EmpresaId`
- [[Módulo Almoxarifado]] — Almoxarifado pode referenciar um `DepartamentoId`

## Configuração

Nenhuma configuração especial — segue o padrão CRUD do `BaseCrudController`.

## Observações importantes

- Invariante: Departamento sempre pertence a uma Empresa
- É o **módulo de referência** para implementar novas features — copiar esta estrutura
- O `EmpresaId` não deve ser setado manualmente no handler — vem automaticamente do contexto
- Tem testes unitários cobrindo Create, Delete, GetById handlers e o validator
