---
tags:
  - orionerp
  - entidade-base
  - auditoria
  - dominio
  - padrao
relacionado:
  - "[[OrionDbContext]]"
  - "[[Módulos/Multi-Tenancy]]"
  - "[[Arquitetura Geral]]"
status: ativo
tipo: componente
versao: 1.0.0
---

# EntityBase e Auditoria

Classe base abstrata da qual todas as entidades de domínio herdam. Define o contrato mínimo de identidade e auditoria. Os campos de auditoria são auto-populados pelo `OrionDbContext.SaveChangesAsync`.

## Como funciona

**`EntityBase`:**
```csharp
public abstract class EntityBase {
    public long Id { get; set; }          // PK — gerada pelo banco
    public DateTime CreatedAt { get; set; } // Auto-populado no insert
    public DateTime? UpdatedAt { get; set; } // Auto-populado no update
    public string? IpAddress { get; set; }  // IP da requisição
    public string? UserAgent { get; set; }  // User-Agent da requisição
}
```

**`ITenantAware`:**
```csharp
public interface ITenantAware {
    string TenantId { get; set; }
}
```
Entidades que implementam `ITenantAware` recebem filtros automáticos no Repository e no DbContext.

**`[IgnoreUpdate]`** — atributo aplicado em campos que não devem ser atualizados via `UpdateExtensions` (ex: `Empresa.Slug`).

**`[IgnoreMap]`** — atributo para excluir propriedades do `ObjectMapper` (ex: campos de senha, navegações).

**Auto-população no `SaveChangesAsync`:**

| Campo | Quando | Fonte |
|---|---|---|
| `CreatedAt` | Insert | `DateTime.UtcNow` |
| `UpdatedAt` | Insert + Update | `DateTime.UtcNow` |
| `EmpresaId` | Insert + Update | `ITenantProvider.EmpresaId` |
| `IpAddress` | Insert + Update | `ITenantProvider.IpAddress` |
| `UserAgent` | Insert + Update | `ITenantProvider.UserAgent` |

## Arquivos principais

| Arquivo | Responsabilidade |
|---|---|
| `OrionERP.Domain/Common/EntityBase.cs` | Classe base de entidades |
| `OrionERP.Domain/Common/Interfaces/ITenantAware.cs` | Interface de tenant scope |
| `OrionERP.Domain/Common/IgnoreUpdateAttribute.cs` | Atributo `[IgnoreUpdate]` |
| `OrionERP.Application/Common/Mapping/IgnoreMappAttribute.cs` | Atributo `[IgnoreMap]` |
| `OrionERP.Application/Extensions/UpdateExtensions.cs` | Aplica update respeitando `[IgnoreUpdate]` |
| `OrionERP.Infrastructure/Persistence/OrionDbContext.cs` | Auto-popula campos no `SaveChangesAsync` |
| `OrionERP.Infrastructure/Persistence/Configurations/AuditableEntityConfiguration.cs` | Configuração EF base para campos auditáveis |

## Integrações

- Todas as entidades de domínio herdam de `EntityBase`
- [[Módulos/Multi-Tenancy]] — `ITenantAware` ativa filtros automáticos
- [[ObjectMapper]] — respeita `[IgnoreMap]`
- `UpdateExtensions` — respeita `[IgnoreUpdate]`

## Configuração

Nenhuma configuração necessária — é automático via herança e convenção.

## Observações importantes

- Todos os `DateTime` são UTC — convertidos automaticamente pelo `UtcDateTimeConverter` do `OrionDbContext`
- `Id` é `long` gerado pelo banco (sequence) — nunca setar manualmente
- Fase 2 prevê adição de `CreatedBy`/`UpdatedBy` para rastreabilidade de usuário
