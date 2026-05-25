# Implementation Rules

These rules are MANDATORY for all code changes in this repository.

They override trends, overengineering, and unnecessary abstraction.

------------------------------------------------------------------------

# 1. DRY -- Don't Repeat Yourself

-   Never duplicate:
    -   Business logic
    -   Validation rules
    -   Mapping logic
    -   Similar queries
    -   Error handling logic

When duplication appears: - Extract extension methods - Create reusable
services (only when truly reusable) - Use base classes carefully - Use
shared DTOs - Prefer composition over duplication

Do not create abstractions prematurely.

------------------------------------------------------------------------

# 2. KISS -- Keep It Simple

Always choose the simplest solution that works correctly.

Avoid: - Overengineering - Unnecessary design patterns - Premature
architecture layers - Heavy libraries when .NET built-in features solve
the problem

Readable and explicit code \> clever code.

------------------------------------------------------------------------

# 3. YAGNI -- You Aren't Gonna Need It

Do NOT implement:

-   Future-proof abstractions
-   Hypothetical extensibility
-   Premature optimization
-   Configurations not currently required

Implement only what delivers value NOW.

Refactor when real needs arise.

------------------------------------------------------------------------

# 4. Feature-Based Folder Organization

Organize by feature/domain --- NOT by file type.

Correct:

Application/ Features/ Orders/ Commands/ Queries/ Dtos/ Validators/

Incorrect:

Application/ Services/ Repositories/ Validators/ Dtos/

Golden Rules:

-   If a file belongs mostly to a feature → it must live inside that
    feature.
-   Avoid giant global folders.
-   Cross-cutting concerns go in:
    -   Common/
    -   CrossCutting/
    -   SharedKernel/

------------------------------------------------------------------------

# 5. Separation of Concerns

Each class must have ONE clear responsibility.

Layer responsibilities:

Domain: - Entities - Value Objects - Domain Events - Domain Services

Application: - Commands - Queries - Handlers - DTOs - Validators

Infrastructure: - EF Core - External APIs - Email - File Storage

API: - Controllers - Minimal APIs - Middleware

Rules:

-   Controllers only orchestrate HTTP layer.
-   Handlers orchestrate use cases.
-   Repositories only access data.
-   Avoid "God Services".

------------------------------------------------------------------------

# 6. Modern C# Standards

-   Use C# 12+
-   Use .NET 9+
-   Prefer:
    -   record
    -   required members
    -   init-only setters
    -   strong typing
-   Avoid weakly typed structures.

------------------------------------------------------------------------

# 7. Validation

-   Use FluentValidation.
-   Validators must live inside the feature.
-   Validation runs through MediatR pipeline.

------------------------------------------------------------------------

# 8. Testing

-   Write unit tests for important behaviors.
-   Integration tests for critical flows.
-   Code must be testable:
    -   Dependency injection
    -   Interfaces when appropriate
    -   No hidden static dependencies

------------------------------------------------------------------------

# 9. Workflow When Generating or Modifying Code

Always follow this sequence:

1.  Plan folder structure first.
2.  Define responsibilities per layer.
3.  Apply DRY / KISS / YAGNI.
4.  Implement.
5.  If a principle is violated, explicitly explain why and suggest
    correction.

------------------------------------------------------------------------

Clean code \> clever code.\
Simple \> complex.\
Explicit \> magical.
