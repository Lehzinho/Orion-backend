# Architectural Decision Log

## 1. ObjectMapper instead of AutoMapper

Reason:

* Lower overhead
* No reflection-heavy runtime behavior
* Deterministic mapping

---

## 2. No UnitOfWork abstraction

Reason:

* EF Core DbContext already acts as UnitOfWork
* Avoid unnecessary abstraction

---

## 3. Direct IRepository<TEntity> injection

Reason:

* Simplicity
* Avoid repository per entity
* Maintain DRY and KISS

---

## 4. Feature-Based Organization

Reason:

* Higher cohesion
* Better scalability
* Easier mental model

---

## 5. Tenant resolution via Subdomain + AsyncLocal

Reason:

* Lightweight
* Avoid external multi-tenant frameworks
* Full control over tenant lifecycle

---

If proposing a change that conflicts with this file,
explicit justification is required.
