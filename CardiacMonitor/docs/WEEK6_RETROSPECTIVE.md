# Week 6 Sprint Retrospective

## What Went Well

- The existing layered structure made it possible to add query behavior without placing database logic in controllers.
- Unit and integration tests provided fast feedback after each change.
- Extracting Fluent API configuration made the schema easier to review.
- The migration check confirmed that deterministic seed data removed continuous model drift.
- The Postman route now demonstrates several query combinations using one endpoint.

## What Needed Improvement

- The old migration named `AddRefreshTokenTable` did not actually create the refresh-token table, so the new migration had to close that schema gap.
- A decimal check constraint that looked correct for SQL Server initially failed in SQLite because the providers store decimals differently.
- The original Postman collection contained incomplete GET request definitions.
- The ERD documentation referenced an image filename that did not exist.

## Concrete Action for the Next Sprint

For every database constraint or transaction added in the next sprint, write or update an integration test against the relational SQLite test database before considering the task complete. This will catch provider behavior and atomicity problems earlier.

## Evidence of Completion

- Build: successful with zero warnings and zero errors.
- Unit tests: 20 passed.
- Integration tests: 11 passed.
- EF Core migration audit: no pending model changes.
