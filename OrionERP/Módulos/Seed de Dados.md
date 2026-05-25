---
tags: [orionerp, seed, dados-iniciais, desenvolvimento, infraestrutura]
relacionado: 
    - "[[Módulo Tenant]]" 
    - "[[Módulo Empresa]]" 
    - "[[Módulo Usuário]]" 
    - "[[OrionDbContext]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# Seed de Dados

População inicial do banco de dados em ambiente de desenvolvimento. Cria Tenant, Empresa, Usuário e EmpresaUsuario padrão para testes locais.

## Como funciona

Executado automaticamente em desenvolvimento via `app.SeedDatabaseAsync()` no `Program.cs`. Verifica se já existem dados (`if (!context.Tenants.Any())`) antes de inserir — idempotente.

**Dados criados:**

| Entidade | Dados |
|---|---|
| Tenant | `Id: "orion"`, `Slug: "orion"`, `Ativo: true` |
| Empresa | `TenantId: "orion"`, `RazaoSocial: "Uni System LTDA"`, `CpfCnpj: "12.345.678/0001-90"`, `Slug: "unisystem"`, `Cidade: "Caldas Novas/GO"` |
| Usuario | `Login: "admin@orion.com"`, `Senha: hash("123mudar")`, `Ativo: true`, `IsAdmin: true` |
| EmpresaUsuario | `TenantId: "orion"`, `Papel: "Admin"` |

**Acesso local:**
- Host: `unisystem.localhost:5000` (slug `unisystem`)
- Login: `admin@orion.com`
- Senha: `123mudar`

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Infrastructure/Persistence/Seeds/DomainSeed.cs` | Lógica de seed síncrona |
| `OrionERP.Infrastructure/Persistence/Seeds/SeedDatabaseAsync.cs` | Extensão assíncrona para `WebApplication` |

## Integrações

- Usa `IPasswordHasherService` para hashear a senha do usuário seed
- Acessa `OrionDbContext` diretamente (sem passar por `ITenantProvider`)

## Configuração

O seed só executa em desenvolvimento (condição `if (app.Environment.IsDevelopment())` no `Program.cs`).

## Observações importantes

- A senha `"123mudar"` é apenas para desenvolvimento — nunca usar em produção
- O seed não substitui migrations — apenas popula dados iniciais após o banco estar criado
- Em ambiente de CI/integração, o `TestWebApplicationFactory` pode ter seed próprio para testes
