# Week 6 Sprint Plan

## Sprint Goal

Improve the Cardiac Monitor API database foundation and core routes by adding explicit EF Core configuration, deterministic migrations, a paginated vital-sign history, transactional identity operations, and verified documentation.

## Sprint Backlog

| Backlog item | Estimate | Definition of done | Status |
| --- | ---: | --- | --- |
| Audit the EF Core model and migration history | 0.5 day | Schema gaps and pending model changes identified | Complete |
| Split Fluent API configurations by entity | 0.5 day | Every domain entity has an explicit configuration | Complete |
| Add constraints, indexes, and deterministic seed data | 1 day | Migration generated and model drift removed | Complete |
| Add vital-sign pagination, filtering, and sorting | 1 day | Endpoint returns a projected paginated DTO | Complete |
| Add registration and refresh-token transactions | 1 day | Multi-step identity writes are atomic | Complete |
| Add scheduling conflict business rules | 0.5 day | Service and database reject exact conflicts | Complete |
| Add automated tests | 0.5 day | Happy paths and error cases pass | Complete |
| Update ERD, Postman, and README documents | 0.5 day | Demo artifacts match the implemented API | Complete |

## Definition of Done

- The project builds with zero warnings and zero errors.
- Unit and integration tests pass.
- The endpoint returns correct status codes and standardized validation errors.
- The EF Core model has no pending migration changes.
- The generated migration has been reviewed before database application.
- ERD, Postman, and README documents match the current implementation.
- Every new method has a concise English `//` responsibility comment.

## Sprint Review Demo

1. Show the updated ERD.
2. Show the entity configuration classes and migration.
3. Run a paginated vital-sign request in Postman.
4. Change heart-rate filters and sorting live.
5. Send an invalid page size and show `ValidationProblemDetails`.
6. Explain the user-registration and refresh-token transaction boundaries.
7. Run the automated test suite and show all passing tests.
