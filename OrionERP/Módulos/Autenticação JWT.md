---
tags:
  - orionerp
  - autenticacao
  - jwt
  - refresh-token
  - seguranca
relacionado:
  - "[[Módulos/Multi-Tenancy]]"
  - "[[Módulo Usuário]]"
  - "[[Módulo Empresa]]"
  - "[[Arquitetura Geral]]"
status: ativo
tipo: feature
versao: 1.0.0
---

# Autenticação JWT

Sistema de autenticação baseado em JWT (JSON Web Token) com suporte a refresh token e revogação. Integrado ao multi-tenancy — o token carrega `TenantId`, `EmpresaId` e papel do usuário.

## Como funciona

**Endpoints:**

| Método | Rota | Descrição |
|---|---|---|
| `POST /api/auth/login` | Login | Autentica credenciais, retorna `AccessToken` + `RefreshToken` |
| `POST /api/auth/refresh` | Renovar token | Troca `RefreshToken` válido por novo `AccessToken` |
| `POST /api/auth/revoke` | Revogar token | Invalida um `RefreshToken` |

**Fluxo de Login (`LoginCommandHandler`):**
1. Valida que o `TenantId` está presente no contexto
2. Busca o `Usuario` pelo login + `TenantId` (com include do `Tenant`)
3. Verifica a senha com `BCrypt` via `IPasswordHasherService`
4. Valida que o usuário está `Ativo`
5. Busca a `Empresa` pelo `EmpresaId` do contexto
6. Verifica o vínculo `EmpresaUsuario` para obter o `Papel`
7. Gera `AccessToken` JWT com claims
8. Gera e persiste `RefreshToken`
9. Retorna `LoginResponse`

**Claims incluídas no JWT:**
- `NameIdentifier` → `usuarioId`
- `Name` → nome do usuário
- `Email` → login do usuário
- `Role` → papel na empresa
- `TenantId`, `TenantSlug`, `EmpresaId`, `EmpresaSlug`, `EmpresaCnpj`, `IsAdmin`

**Configuração JWT (`JwtSettings`):**
- `Secret` — chave HMAC-SHA256
- `ExpiryMinutes` — duração do Access Token
- `RefreshTokenExpirationDays` — duração do Refresh Token
- `Issuer`, `Audience`

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.API/Controllers/AuthController.cs` | Endpoints de login, refresh e revogação |
| `OrionERP.Application/Features/Auth/Login/Commands/LoginCommandHandler.cs` | Lógica de autenticação |
| `OrionERP.Application/Features/Auth/Login/Commands/LoginCommandValidator.cs` | Validação de entrada |
| `OrionERP.Application/Features/Auth/RefreshAccessToken/Commands/RefreshAccessTokenHandler.cs` | Renovação de token |
| `OrionERP.Application/Features/Auth/RevokeRefreshToken/Commands/RevokeRefreshTokenHandler.cs` | Revogação |
| `OrionERP.Infrastructure/Services/TokenServices.cs` | Geração e validação de JWT + RefreshToken |
| `OrionERP.Infrastructure/Services/PasswordHasherService.cs` | Hash/verify de senha com BCrypt |
| `OrionERP.API/Extensions/AuthExtensions.cs` | Configuração do middleware de autenticação JWT |
| `OrionERP.Application/Common/Models/JwtSettings.cs` | Modelo de configuração |
| `OrionERP.Domain/Entities/RefreshToken.cs` | Entidade de refresh token (global, sem tenant scope) |

## Integrações

- [[Módulos/Multi-Tenancy]] — o tenant e a empresa já estão resolvidos pelo `TenantMiddleware` antes do login
- `IRepository<RefreshToken>` — persiste tokens no banco
- `IRepository<EmpresaUsuario>` — verifica vínculo usuário-empresa para obter papel
- `BaseCrudController` aplica `[Authorize]` em todos os controllers derivados

## Configuração

Em `appsettings.json`:
```json
{
  "JwtSettings": {
    "Secret": "...",
    "ExpiryMinutes": 60,
    "RefreshTokenExpirationDays": 7,
    "Issuer": "OrionERP",
    "Audience": "OrionERP"
  }
}
```

## Observações importantes

- O `AuthController` não herda de `BaseCrudController` — tem operações específicas de autenticação
- O token JWT expira em minutos, o `RefreshToken` em dias
- `RefreshToken` é global (sem filtro de tenant) — contém `TenantId` e `EmpresaId` como dados
- Credenciais inválidas retornam `401` com mensagem genérica (sem vazar qual campo falhou)
- O papel (`Role`) é determinado pela tabela `EmpresaUsuario`, não pelo usuário global
