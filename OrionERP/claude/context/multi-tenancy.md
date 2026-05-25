# Multi-Tenancy Rules (CRITICAL)

OrionERP is strictly multi-tenant.

---

## Mandatory Rules

* Every tenant-scoped entity MUST implement ITenantAware
* Never manually set EmpresaId inside handlers
* Never bypass TenantProvider
* Never disable global query filters
* Never use IgnoreQueryFilters() unless explicitly justified

---

## Tenant Flow

1. TenantMiddleware resolves tenant from hostname slug
2. ITenantProvider stores context using AsyncLocal
3. DbContext applies global query filters
4. SaveChangesAsync auto-populates:

   * EmpresaId
   * CreatedAt
   * UpdatedAt
   * IpAddress
   * UserAgent

---

## Data Safety

Under no circumstance should data from one tenant be accessible by another.

If a new entity is added, verify:

* Is it tenant-scoped?
* Does it require EmpresaId?
* Does it need global filtering?

Multi-tenant safety validation is mandatory in every new feature.
