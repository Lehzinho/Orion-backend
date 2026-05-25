---
tags: [orionerp, empresa-usuario, permissoes, multi-tenancy, relacao]
relacionado: 
    - "[[Módulo Empresa]]" 
    - "[[Módulo Usuário]]" 
    - "[[Autenticação JWT]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Módulo EmpresaUsuario

Tabela de relacionamento N:N entre `Empresa` e `Usuario`. Define o papel (`Role`) de um usuário dentro de uma empresa específica. Sem esse vínculo, o usuário não pode fazer login em nenhuma empresa.

## Como funciona

**Entidade `EmpresaUsuario`:**
- TenantScoped + EmpresaScoped
- Campos: `TenantId`, `EmpresaId`, `UsuarioId`, `Papel` (string, ex: `"Admin"`, `"Operador"`)
- FKs: para `Empresa` e para `Usuario`
- Sem CRUD exposto via API no estado atual — gerenciado via seed e internamente

**Papel no login:**

Durante o `LoginCommandHandler`, o `EmpresaUsuario` é consultado para:
1. Confirmar que o usuário tem acesso à empresa resolvida pelo subdomínio
2. Obter o `Papel` que será incluído como `Role` no JWT

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Entities/EmpresaUsuario.cs` | Entidade de domínio |
| `OrionERP.Infrastructure/Persistence/Configurations/EmpresaUsuarioConfiguration.cs` | Configuração EF |

## Integrações

- [[Autenticação JWT]] — determina o papel do usuário no token JWT
- [[Módulo Empresa]] — FK para `Empresa`
- [[Módulo Usuário]] — FK para `Usuario`

## Configuração

Não há CRUD exposto atualmente. Os vínculos são criados via seed (`DomainSeed`) ou diretamente no banco.

## Observações importantes

- Um usuário sem `EmpresaUsuario` para a empresa acessada recebe `401 Unauthorized` no login
- Fase 2 prevê permissões avançadas via `EmpresaUsuario` (ACL mais granular)
- O `Papel` é uma string livre atualmente — sem enum validado
