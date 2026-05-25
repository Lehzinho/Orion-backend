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

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "orionpos";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "orionpos-app";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "development-super-secret-key-change-me";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "OrionPOS API";
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("Sistema")
    .ExcludeFromDescription();

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
.Produces(403)
.Produces(404)
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

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrionPosDbContext>();
    await DbInitializer.SeedAsync(dbContext);
}

app.Run();
