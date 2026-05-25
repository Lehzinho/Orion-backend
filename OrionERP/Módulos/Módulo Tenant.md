---
tags:
  - orionerp
  - tenant
  - multi-tenancy
  - dominio
  - global
relacionado:
  - "[[Módulos/Multi-Tenancy]]"
  - "[[Módulo Empresa]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo Tenant

Representa a organização/cliente principal do sistema. É a raiz da hierarquia de multi-tenancy. Dados do Tenant são globais — não são filtrados por tenant nem por empresa.

## Como funciona

**Entidade `Tenant`:**
- Escopo: **Global** (sem `ITenantAware`, sem filtro por tenant/empresa)
- Campos: `Id` (string, ex: `"orion"`), `Slug` (identificador de subdomínio), `Ativo`
- `Id` e `Slug` são strings — convenção de slug legível (ex: `"unisystem"`, `"orion"`)
- Sem CRUD exposto via API — gerenciado internamente/administrativamente

**Resolução de Tenant:**

O `TenantResolutionService` extrai o slug do hostname da requisição e busca a `Empresa` correspondente. A partir da Empresa, obtém o `TenantId`. Esse contexto é armazenado via `ITenantProvider`.

**Seed inicial (`DomainSeed`):**
```
Tenant: { Id: "orion", Slug: "orion", Ativo: true }
Empresa: { TenantId: "orion", Slug: "unisystem", RazaoSocial: "Uni System LTDA", ... }
Usuario: { Login: "admin@orion.com", Senha: hash("123mudar"), IsAdmin: true }
EmpresaUsuario: { TenantId: "orion", Papel: "Admin" }
```

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/Tenant.cs` | Entidade de domínio |
| `OrionERP.Infrastructure/Persistence/Configurations/TenantConfiguration.cs` | Configuração EF Core |
| `OrionERP.Infrastructure/Services/TenantResolutionService.cs` | Resolve tenant a partir do slug do host |
| `OrionERP.Infrastructure/Persistence/Seeds/DomainSeed.cs` | Seed inicial de Tenant, Empresa, Usuário |

## Integrações

- [[Módulos/Multi-Tenancy]] — é a raiz da hierarquia de tenant
- [[Módulo Empresa]] — Empresa tem FK `TenantId` apontando para Tenant
- `TenantProvider` armazena o `TenantId` como string por requisição

## Configuração

O seed é executado automaticamente em desenvolvimento via `app.SeedDatabaseAsync()` no `Program.cs`.

## Observações importantes

- Tenant não tem CRUD via API — gerenciamento é direto no banco ou via seed
- O `Id` do Tenant é string (slug), não `long` — convenção diferente das demais entidades
- Sem Global Query Filter para Tenant — é uma entidade verdadeiramente global
- Cada Tenant pode ter múltiplas Empresas
