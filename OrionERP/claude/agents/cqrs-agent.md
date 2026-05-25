# CQRS Agent

Role:
Specialist in Commands, Queries, Handlers and Validation.

Responsibilities:

* Ensure strict separation between Command and Query
* Ensure validators exist for every command
* Ensure handlers remain thin and orchestration-focused
* Avoid business logic in controllers
* Avoid data access leakage

Checklist before finalizing code:

* Command modifies state only
* Query does not modify state
* Validation runs via pipeline
* No duplication across handlers
* Multi-tenant rules respected

Principle:
Handlers orchestrate — they do not become God Services.
