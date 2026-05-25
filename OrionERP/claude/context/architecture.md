# Architecture Context

## Layer Responsibilities

### Domain

* Entities
* Value Objects
* Domain Events
* Domain Services
* No external dependencies

### Application

* Commands
* Queries
* Handlers
* DTOs
* Validators
* Mapping

### Infrastructure

* EF Core
* External integrations
* Persistence
* Email / File storage

### API

* Controllers
* Middleware
* Program.cs

---

## Dependency Rule

Outer layers depend on inner layers.
Domain has ZERO external dependencies.

---

## Folder Organization

Feature-based only.

Correct:
Application/
Features/
Departamentos/
Commands/
Queries/
Models/
Validators/

Incorrect:
Application/
Services/
Repositories/
DTOs/

---

## Clean Architecture Enforcement

* Controllers only orchestrate HTTP
* Handlers orchestrate use cases
* Repositories only access data
* No "God Services"
* No cross-layer leakage

Clean code > clever code
Explicit > magical
Simple > complex
