---
tags:
  - orionerp
  - usuario
  - crud
  - autenticacao
  - multi-tenancy
relacionado:
  - "[[Autenticação JWT]]"
  - "[[Módulos/Multi-Tenancy]]"
  - "[[Módulo Empresa]]"
  - "[[Módulo EmpresaUsuario]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Usuário

Representa um usuário do sistema pertencente a um Tenant. Um usuário pode ter acesso a múltiplas empresas dentro do mesmo tenant, com papéis diferentes em cada uma via `EmpresaUsuario`.

## Como funciona

**Entidade `Usuario`:**
- TenantScoped (`ITenantAware`) — filtro global por `TenantId`
- **Não** EmpresaScoped — um usuário existe no nível do Tenant, não de uma Empresa específica
- Campos: `TenantId`, `Login`, `Senha` (hash BCrypt), `Nome`, `Ativo`, `IsAdmin`, `ExternalId`
- Relacionamentos: tem muitos `EmpresaUsuario` (vínculo com empresas)

**CRUD completo via `BaseCrudController`:**

| Endpoint | Rota |
|---|---|
| Criar | `POST /api/usuario` |
| Buscar | `GET /api/usuario/{id}` |
| Listar | `GET /api/usuario?pageNumber=1&pageSize=20` |
| Atualizar | `PUT /api/usuario/{id}` |
| Deletar | `DELETE /api/usuario/{id}` |

**Criação de usuário:**
- A senha é hasheada via `IPasswordHasherService` (BCrypt) no `CreateUsuarioCommandHandler`
- Verificação de unicidade de login via `IUsuarioUniqueChecker`

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/Usuario.cs` | Entidade de domínio |
| `OrionERP.Infrastructure/Persistence/Configurations/UsuarioConfiguration.cs` | Configuração EF |
| `OrionERP.API/Controllers/UsuarioController.cs` | Controller HTTP |
| `OrionERP.Application/Features/Usuarios/Commands/` | Create, Update, Delete handlers + validators |
| `OrionERP.Application/Features/Usuarios/Queries/` | GetById, GetAll handlers |
| `OrionERP.Application/Features/Usuarios/Models/` | DTOs, Requests, Responses |
| `OrionERP.Application/Common/Interfaces/IUsuarioUniqueChecker.cs` | Contrato de unicidade de login |
| `OrionERP.Infrastructure/Services/UsuarioUniqueChecker.cs` | Implementação |
| `OrionERP.Infrastructure/Services/PasswordHasherService.cs` | Hash/verify BCrypt |

## Integrações

- [[Autenticação JWT]] — `LoginCommandHandler` busca `Usuario` pelo login + TenantId
- [[Módulos/Multi-Tenancy]] — filtro global por `TenantId` no `OrionDbContext`
- [[Módulo EmpresaUsuario]] — vínculo N:N com empresas

## Configuração

A senha nunca é retornada nos responses — os models de resposta não incluem o campo `Senha`.

## Observações importantes

- Invariante: um `Usuario` pode existir sem vínculo com nenhuma empresa (mas não consegue fazer login sem `EmpresaUsuario`)
- `IsAdmin` representa admin global do tenant — não confundir com papel na empresa (definido em `EmpresaUsuario`)
- `Login` é o email do usuário e deve ser único dentro do tenant
- A senha nunca é exposta em nenhum response
