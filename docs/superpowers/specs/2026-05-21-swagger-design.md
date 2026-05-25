# Spec: Swagger / OpenAPI — OrionPOS Backend

**Data:** 2026-05-21
**Status:** Aprovado

---

## Contexto

O backend OrionPOS é uma ASP.NET Core 9 Web API em Minimal APIs com autenticação JWT Bearer. O pacote `Microsoft.AspNetCore.OpenApi` já está instalado e `AddOpenApi()` / `MapOpenApi()` já estão configurados, servindo o spec JSON em `/openapi/v1.json`. Falta: UI interativa, metadata nos endpoints e documentação do security scheme.

## Objetivo

Expor documentação interativa (Swagger UI) sem proteção de acesso, em todos os ambientes, cobrindo os endpoints existentes com tipos de resposta, tags e descrições — e com suporte nativo ao JWT Bearer (botão "Authorize").

---

## Abordagem escolhida: Scalar + metadata nos endpoints

Motivo: o projeto já usa `Microsoft.AspNetCore.OpenApi` (nativo do .NET 9). O Scalar é a UI recomendada pela Microsoft para .NET 9 e requer apenas um pacote extra. Zero retrabalho ou substituição de infraestrutura existente.

---

## Escopo da implementação

### 1. Pacote

Adicionar ao `OrionPOS.Api.csproj`:

```xml
<PackageReference Include="Scalar.AspNetCore" Version="2.*" />
```

### 2. `Program.cs` — security scheme JWT

Substituir `builder.Services.AddOpenApi()` por:

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

### 3. `Program.cs` — expor UI Scalar

Remover o bloco `if (app.Environment.IsDevelopment())` e substituir por:

```csharp
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "OrionPOS API";
    options.Theme = ScalarTheme.Default;
});
```

A UI fica disponível em `/scalar/v1` em todos os ambientes, sem proteção.

### 4. Metadata dos endpoints

#### `GET /health`
```csharp
.WithTags("Sistema")
.ExcludeFromDescription()
```

#### `POST /auth/login`
```csharp
.WithTags("Auth")
.WithSummary("Autenticar usuário")
.WithDescription("Valida email e senha e retorna um JWT Bearer para uso nas rotas protegidas.")
.Produces<LoginResponseDto>(200)
.Produces(401)
.WithOpenApi()
```

#### `GET /auth/me`
```csharp
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
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme } }]
                = Array.Empty<string>()
        }
    };
    return op;
})
```

#### `GET /cardapio`
```csharp
.WithTags("Catálogo")
.WithSummary("Cardápio completo do tenant")
.WithDescription("Retorna o cardápio com seções, produtos, variantes e grupos de modificadores.")
.Produces<CardapioDto>(200)
.Produces(404)
.WithOpenApi()
```

---

## Resultado esperado

| Rota | O que serve |
|------|-------------|
| `/scalar/v1` | UI interativa (Scalar) com botão Authorize |
| `/openapi/v1.json` | Spec OpenAPI 3.0 em JSON |

### Grupos na UI
- **Auth** — `POST /auth/login`, `GET /auth/me` (cadeado fechado)
- **Catálogo** — `GET /cardapio`
- `GET /health` — oculto

---

## Fora do escopo

- Exemplos de request/response inline (pode ser adicionado por endpoint no futuro com `.WithOpenApi(op => { ... })`)
- Endpoints de comandas, mesas e pagamentos — ainda não existem; serão documentados quando implementados seguindo o mesmo padrão de metadata
- Versionamento da API (`/openapi/v2.json`)
