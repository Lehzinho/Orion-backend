# Swagger com Scalar — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expor documentação OpenAPI interativa via Scalar UI em `/scalar/v1`, com JWT Bearer documentado e metadata em todos os endpoints existentes.

**Architecture:** O projeto já usa `Microsoft.AspNetCore.OpenApi` (nativo .NET 9) que gera o spec JSON. Adicionamos `Scalar.AspNetCore` para a UI, configuramos o security scheme JWT via `AddDocumentTransformer`, e anotamos os 4 endpoints com `.WithTags()`, `.WithSummary()`, `.Produces<T>()`.

**Tech Stack:** .NET 9 Minimal APIs, `Microsoft.AspNetCore.OpenApi` (já instalado), `Scalar.AspNetCore` (a adicionar)

---

## Arquivos alterados

| Arquivo | Ação |
|---|---|
| `OrionPOS.Api/OrionPOS.Api.csproj` | Modificar — adicionar `Scalar.AspNetCore` |
| `OrionPOS.Api/Program.cs` | Modificar — security scheme, Scalar UI, metadata dos endpoints |

---

## Task 1: Adicionar pacote Scalar.AspNetCore

**Files:**
- Modify: `OrionPOS.Api/OrionPOS.Api.csproj`

- [ ] **Step 1: Adicionar PackageReference no csproj**

Abrir `OrionPOS.Api/OrionPOS.Api.csproj` e adicionar dentro do primeiro `<ItemGroup>`:

```xml
<PackageReference Include="Scalar.AspNetCore" Version="2.*" />
```

O bloco `<ItemGroup>` final deve ficar assim:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.9" />
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.9" />
  <PackageReference Include="Scalar.AspNetCore" Version="2.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.9">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

- [ ] **Step 2: Restaurar pacotes**

```bash
cd OrionPOS.Api
dotnet restore
```

Esperado: `Restore completed` sem erros.

- [ ] **Step 3: Build para confirmar que o pacote foi resolvido**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 2: Configurar security scheme JWT e expor Scalar UI

**Files:**
- Modify: `OrionPOS.Api/Program.cs`

- [ ] **Step 1: Adicionar usings no topo do Program.cs**

Substituir o bloco de usings existente por:

```csharp
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using OrionPOS.Application;
using OrionPOS.Application.Auth;
using OrionPOS.Application.Catalogo;
using OrionPOS.Infra;
using OrionPOS.Infra.Persistence;
using Scalar.AspNetCore;
```

- [ ] **Step 2: Substituir `AddOpenApi()` simples pela versão com security scheme**

Localizar:

```csharp
builder.Services.AddOpenApi();
```

Substituir por:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, context, ct) =>
    {
        doc.Components ??= new();
        doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Insira o token JWT obtido em POST /auth/login"
        };
        return Task.CompletedTask;
    });
});
```

- [ ] **Step 3: Substituir o bloco `if (IsDevelopment)` pela exposição irrestrita**

Localizar:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

Substituir por:

```csharp
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "OrionPOS API";
});
```

- [ ] **Step 4: Build para confirmar que compila**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 3: Adicionar metadata nos endpoints

**Files:**
- Modify: `OrionPOS.Api/Program.cs`

Esta task substitui os 4 endpoints existentes pelas versões anotadas. Substitua cada bloco como descrito abaixo.

- [ ] **Step 1: Anotar `GET /health`**

Localizar:

```csharp
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
```

Substituir por:

```csharp
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("Sistema")
    .ExcludeFromDescription();
```

- [ ] **Step 2: Anotar `POST /auth/login`**

Localizar:

```csharp
app.MapPost("/auth/login", async (LoginRequestDto request, LoginUseCase useCase, CancellationToken ct) =>
{
    var result = await useCase.ExecuteAsync(request, ct);
    return result is null
        ? Results.Unauthorized()
        : Results.Ok(result);
});
```

Substituir por:

```csharp
app.MapPost("/auth/login", async (LoginRequestDto request, LoginUseCase useCase, CancellationToken ct) =>
{
    var result = await useCase.ExecuteAsync(request, ct);
    return result is null
        ? Results.Unauthorized()
        : Results.Ok(result);
})
.WithTags("Auth")
.WithSummary("Autenticar usuário")
.WithDescription("Valida email e senha e retorna um JWT Bearer para uso nas rotas protegidas.")
.Produces<LoginResponseDto>(200)
.Produces(401)
.WithOpenApi();
```

- [ ] **Step 3: Anotar `GET /auth/me`**

Localizar:

```csharp
app.MapGet("/auth/me", async (ClaimsPrincipal principal, GetMeUseCase useCase, CancellationToken ct) =>
{
    var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      principal.FindFirstValue("sub");

    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    var me = await useCase.ExecuteAsync(userId, ct);
    return me is null ? Results.NotFound() : Results.Ok(me);
}).RequireAuthorization();
```

Substituir por:

```csharp
app.MapGet("/auth/me", async (ClaimsPrincipal principal, GetMeUseCase useCase, CancellationToken ct) =>
{
    var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      principal.FindFirstValue("sub");

    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();
    }

    var me = await useCase.ExecuteAsync(userId, ct);
    return me is null ? Results.NotFound() : Results.Ok(me);
})
.RequireAuthorization()
.WithTags("Auth")
.WithSummary("Dados do usuário autenticado")
.WithDescription("Retorna os dados do usuário dono do token JWT informado no header Authorization.")
.Produces<MeResponseDto>(200)
.Produces(401)
.WithOpenApi(op =>
{
    op.Security = new List<OpenApiSecurityRequirement>
    {
        new()
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            }] = Array.Empty<string>()
        }
    };
    return op;
});
```

- [ ] **Step 4: Anotar `GET /cardapio`**

Localizar:

```csharp
app.MapGet("/cardapio", async (GetCardapioUseCase useCase, IConfiguration configuration, CancellationToken ct) =>
{
    var tenantId = Guid.TryParse(configuration["Catalog:DevTenantId"], out var parsed)
        ? parsed
        : DbInitializer.DevTenantId;
    var result = await useCase.ExecuteAsync(tenantId, ct);
    return result is null ? Results.NotFound() : Results.Ok(result);
});
```

Substituir por:

```csharp
app.MapGet("/cardapio", async (GetCardapioUseCase useCase, IConfiguration configuration, CancellationToken ct) =>
{
    var tenantId = Guid.TryParse(configuration["Catalog:DevTenantId"], out var parsed)
        ? parsed
        : DbInitializer.DevTenantId;
    var result = await useCase.ExecuteAsync(tenantId, ct);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.WithTags("Catálogo")
.WithSummary("Cardápio completo do tenant")
.WithDescription("Retorna o cardápio com seções, produtos, variantes e grupos de modificadores.")
.Produces<CardapioDto>(200)
.Produces(404)
.WithOpenApi();
```

- [ ] **Step 5: Build final**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Warning(s). 0 Error(s).`

---

## Task 4: Verificação manual

**Files:** nenhum

- [ ] **Step 1: Subir a API**

```bash
cd OrionPOS.Api
dotnet run
```

Anotar a porta exibida no output (ex: `Now listening on: http://localhost:5171`).

- [ ] **Step 2: Verificar que o spec JSON está disponível**

```bash
curl http://localhost:<porta>/openapi/v1.json
```

Esperado: JSON com `"openapi": "3.0..."` contendo `"securitySchemes": { "Bearer": {...} }`.

- [ ] **Step 3: Verificar que a Scalar UI abre**

Abrir no browser: `http://localhost:<porta>/scalar/v1`

Esperado:
- Página Scalar carrega com título "OrionPOS API"
- Grupos **Auth** e **Catálogo** visíveis
- `GET /health` **não aparece** na lista
- Botão **Authorize** visível no topo

- [ ] **Step 4: Verificar cadeado no endpoint protegido**

Na Scalar UI, expandir `GET /auth/me`.

Esperado: ícone de cadeado fechado indicando que o endpoint requer Bearer token.

- [ ] **Step 5: Testar fluxo completo na UI**

1. Clicar em **Authorize**, inserir um token Bearer válido (obter via `POST /auth/login` com as credenciais seed)
2. Executar `POST /auth/login` com body `{ "email": "admin@orion.com", "password": "123" }` (credenciais do seed em `DbInitializer.cs`)
3. Copiar o `accessToken` retornado
4. Clicar em **Authorize** e inserir `<token>`
5. Executar `GET /auth/me`

Esperado: resposta `200` com `{ "id": "...", "email": "...", "createdAt": "..." }`.
