# Patterns & Conventions

## CQRS

* Commands mutate state
* Queries read state
* Handlers orchestrate use cases
* Validation via FluentValidation pipeline behavior

---

## Object Mapping

* Use ObjectMapper
* Avoid AutoMapper
* Use expression-based mapping
* Use [IgnoreMap] where necessary

---

## Repository Usage

Inject IRepository<TEntity> directly in handlers.

Available methods:

* GetPagedListAsync
* FindAsync
* AsQueryable

Avoid additional repository abstractions.

---

## Exception Handling

Throw domain exceptions inside handlers.
Middleware maps them to HTTP responses.

---

## Language Convention

* Domain terms in Portuguese
* Infrastructure/framework terms in English

Consistency is mandatory.
