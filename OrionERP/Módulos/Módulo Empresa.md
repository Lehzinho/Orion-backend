---
tags:
  - orionerp
  - empresa
  - crud
  - multi-tenancy
  - dominio
relacionado:
  - "[[Módulos/Multi-Tenancy]]"
  - "[[Módulo Tenant]]"
  - "[[Módulo Departamento]]"
  - "[[Módulo Almoxarifado]]"
  - "[[Módulo PDV]]"
  - "[[BaseCrudController]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Empresa

Representa a unidade de negócio dentro de um Tenant. Toda operação de negócio do sistema está associada a uma Empresa. É o nível de isolamento primário para dados operacionais (departamentos, almoxarifados, PDV, etc.).

## Como funciona

**Entidade `Empresa`:**
- Pertence a um `Tenant` (TenantScoped via `ITenantAware`)
- Possui `Slug` único — usado para resolução de tenant via subdomínio (ex: `empresaA.orionerp.com`)
- `Slug` tem `[IgnoreUpdate]` — nunca é alterado após criação
- Campos: `RazaoSocial`, `NomeFantasia`, `CpfCnpj`, `InscEstadual`, `Slug`, `Email`, `Telefone1/2/3`, `Cep`, `Pais`, `Estado`, `Cidade`, `Bairro`, `Endereco`, `Complemento`, `Numero`, `Ativo`, `Contato`, `ExternalId`
- Relacionamentos: tem muitos `Almoxarifados`, `CentrosCustos`, `Departamentos`, `EmpresaUsuarios`, `Pdvs`

**CRUD completo via `BaseCrudController`:**

| Endpoint | Rota |
|---|---|
| Criar | `POST /api/empresa` |
| Buscar | `GET /api/empresa/{id}` |
| Listar | `GET /api/empresa?RazaoSocial.contains=...&pageNumber=1&pageSize=20` |
| Atualizar | `PUT /api/empresa/{id}` |
| Deletar | `DELETE /api/empresa/{id}` |

**Validação de unicidade:** `IEmpresaUniqueChecker` verifica duplicidade de `CpfCnpj` e `Slug` no create/update.

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/Empresa.cs` | Entidade de domínio |
| `OrionERP.Infrastructure/Persistence/Configurations/EmpresaConfiguration.cs` | Configuração EF Core (tabela, índices) |
| `OrionERP.API/Controllers/EmpresaController.cs` | Controller HTTP |
| `OrionERP.Application/Features/Empresas/Commands/` | Create, Update, Delete handlers + validators |
| `OrionERP.Application/Features/Empresas/Queries/` | GetById, GetAll handlers |
| `OrionERP.Application/Features/Empresas/Models/` | DTOs, Request e Response models |
| `OrionERP.Application/Common/Interfaces/IEmpresaUniqueChecker.cs` | Contrato de verificação de unicidade |
| `OrionERP.Infrastructure/Services/EmpresaUniqueChecker.cs` | Implementação da verificação |

## Integrações

- [[Módulos/Multi-Tenancy]] — filtro global por `TenantId` via `OrionDbContext`
- [[Autenticação JWT]] — `LoginCommandHandler` busca a Empresa pelo `EmpresaId` do contexto
- [[Módulo Tenant]] — `Empresa.TenantId` é FK para `Tenant`
- [[Módulo Departamento]], [[Módulo Almoxarifado]], [[Módulo PDV]] — todos têm FK para `EmpresaId`

## Configuração

Nenhuma configuração especial. A Empresa é criada via CRUD autenticado. O Slug deve ser definido no create e não pode ser alterado.

## Observações importantes

- Invariante: Empresa sempre pertence a um Tenant
- O `Slug` é imutável após criação — usado como identificador de subdomínio
- Verificar unicidade de `CpfCnpj` via `IEmpresaUniqueChecker` antes de criar/atualizar
- O `ExternalId` é campo reservado para integração com sistema legado Delphi
