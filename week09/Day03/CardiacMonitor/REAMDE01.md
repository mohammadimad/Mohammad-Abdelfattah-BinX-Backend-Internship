# Cardiac Monitor API — Week 9 Day 1

## Sprint 4 Planning and Test Coverage Audit

Day 1 starts Sprint 4 by auditing the API endpoint by endpoint, ranking missing tests by risk, and closing five priority gap groups. The work adds real HTTP integration tests for authentication failures, refresh-token rotation and replay, Patient resource ownership, privileged write authorization, and the Patient catalog SQL-query budget.

The full suite passes: **34 unit tests and 49 integration tests, 83 tests in total**. This is an increase of **31 integration test cases** from the Day 1 baseline. Remaining endpoint gaps are recorded below; a green suite does not mean that every endpoint already meets the happy-path plus error-path baseline.

## Task 1 — Complete Sprint 4 Planning

### Sprint goal

Deliver a capstone API with critical routes protected by passing unit and integration tests, documented request and response contracts, an automated build-and-test pipeline, and a verified deployment. Preserve Sprint 3 query optimizations with an automated regression check.

### Carry-forward action from Sprint 3

The retrospective requested a regression test that prevents an N+1 query from returning to the primary catalog endpoint. The current implementation of `PatientService.GetAllPatientsAsync` deliberately executes **two** SQL commands:

1. A `COUNT` query for pagination metadata.
2. A projected query for the requested page.

Therefore, the correct assertion is **two queries**, not one. The new test checks the actual HTTP endpoint with 62 Patient records and page sizes of 1 and 50. The count must remain two as the number of returned records grows.

### Sprint backlog

| Priority | Backlog item | Acceptance criteria | Day 1 status |
| --- | --- | --- | --- |
| P0 | Audit every business API endpoint | Record happy-path and error-path evidence for all 39 routes. | Complete |
| P0 | Close authentication failures | Invalid credentials and duplicate registration produce safe errors. | Complete |
| P0 | Cover refresh-token lifecycle | A valid expired access-token pair rotates once; replay and malformed input fail. | Complete |
| P0 | Verify Patient resource isolation | Patient reads allow the owner and reject another Patient's resources. | Complete for the six tested read routes |
| P0 | Protect privileged write routes | A Patient token receives 403 before database work occurs. | Complete for 19 tested write routes |
| P1 | Protect the catalog query budget | Small and large result pages both execute exactly two SQL commands. | Complete |
| P1 | Finish critical endpoint happy paths and error cases | Cover the remaining gaps listed in the audit and add branch-specific assertions. | Remaining |
| P1 | Add booking and alert edge cases | Verify duplicate/concurrent bookings and invalid alert-state transitions. | Remaining |
| P1 | Document and automate verification | Publish API examples and add build/test checks to CI. | Planned for Sprint 4 |
| P1 | Deploy and smoke-test | Configure secrets, database and Redis; verify protected routes in the deployed environment. | Planned for Sprint 4 |

Daily stand-ups track completed tests, the next highest-risk gap, and blockers. The mentor review should include the route audit, test changes, query-budget assertion, and passing test output.

## Task 2 — Audit Every Endpoint

### Audit rules

- **B — Both:** at least one passing happy-path HTTP test and one realistic error-path HTTP test.
- **H — Happy only:** successful HTTP behavior is tested; an error-path test is missing.
- **E — Error only:** failure behavior is tested; a happy-path HTTP test is missing.
- **N — Neither:** neither HTTP path is covered in the audited test suite.

The inventory comes from the nine controller classes under `Controllers/`. Route constraints such as `{id:int}` are simplified to `{id}` in this table. Swagger UI and middleware behavior are not additional business endpoints.

This table measures explicit HTTP assertions, not line-coverage percentages. Existing unit tests remain valuable supporting evidence, but a validator or service test alone does not verify controller routing, model binding, JWT middleware, authorization, or HTTP serialization. A successful empty-list response counts as a happy path, but does not prove populated-list mapping. One error case does not cover every error branch.

### Evidence key

Existing HTTP tests are in `tests/CardiacMonitor.IntegrationTests/ApiEndpointsTests.cs`:

| Code | Existing evidence |
| --- | --- |
| P | Patient list, filtered pagination, invalid page size, missing Patient, and ownership rejection. |
| V | Vital-sign filtered pagination, invalid page size, invalid readings, and critical-reading creation. |
| R | Successful registration, injected-role restriction, and successful login with the Patient ID claim. |
| NUR | Nurse creation, assignment creation, assigned Patient access, own Patient list, and forbidden Nurse creation. |
| DOC | Doctor creation, availability creation, successful appointment booking, and rejection outside availability. |
| ALT | Alert listing, acknowledgement, and resolution after a critical reading. |
| AP | Appointment rejection when the selected identity is not a Doctor. |

New HTTP tests are in `tests/CardiacMonitor.IntegrationTests/Sprint4Day1Tests.cs`:

| Code | New evidence |
| --- | --- |
| AUTH | `Login_RejectsInvalidCredentials` and `Register_RejectsDuplicateEmail_WithoutCreatingAnotherProfile`. |
| REF | `Refresh_RotatesTokenPair_AndRejectsReplay` and `Refresh_RejectsMalformedAccessToken`. |
| OWN | `PatientReads_EnforceResourceOwnership`, six route cases. |
| ROLE | `PrivilegedWrites_RejectPatientRole`, 19 route cases. |
| SQL | `PatientCatalog_QueryCountRemainsTwo_ForSmallAndLargePages`. |

### Complete route inventory

| Method | Route | Before | After | Evidence |
| --- | --- | --- | --- | --- |
| POST | `/api/auth/register` | H | B | R, AUTH |
| POST | `/api/auth/login` | H | B | R, AUTH |
| POST | `/api/auth/refresh` | N | B | REF |
| GET | `/api/patients` | B | B | P, SQL |
| GET | `/api/patients/{id}` | B | B | P, NUR, OWN |
| GET | `/api/patients/{id}/clinical-details` | N | B | OWN |
| POST | `/api/patients` | N | E | ROLE |
| PUT | `/api/patients/{id}` | N | E | ROLE |
| DELETE | `/api/patients/{id}` | N | E | ROLE |
| GET | `/api/patients/{patientId}/vitals` | B | B | V, OWN |
| POST | `/api/patients/{patientId}/vitals` | B | B | V, ALT |
| GET | `/api/vitals/{id}` | N | N | Remaining |
| PUT | `/api/vitals/{id}` | N | E | ROLE |
| DELETE | `/api/vitals/{id}` | N | E | ROLE |
| GET | `/api/patients/{patientId}/medications` | N | B | OWN |
| POST | `/api/patients/{patientId}/medications` | N | E | ROLE |
| GET | `/api/medications/{id}` | N | N | Remaining |
| PUT | `/api/medications/{id}` | N | E | ROLE |
| DELETE | `/api/medications/{id}` | N | E | ROLE |
| GET | `/api/patients/{patientId}/appointments` | N | B | OWN |
| POST | `/api/patients/{patientId}/appointments` | B | B | DOC, AP, ROLE |
| GET | `/api/appointments/{id}` | N | N | Remaining |
| PUT | `/api/appointments/{id}` | N | E | ROLE |
| DELETE | `/api/appointments/{id}` | N | E | ROLE |
| POST | `/api/staff/nurses` | B | B | NUR, ROLE |
| GET | `/api/staff/nurses` | N | N | Remaining |
| POST | `/api/staff/doctors` | H | B | DOC, ROLE |
| GET | `/api/staff/doctors` | N | N | Remaining |
| POST | `/api/patients/{patientId}/care-assignments` | H | B | NUR, ROLE |
| GET | `/api/patients/{patientId}/care-assignments` | N | N | Remaining |
| GET | `/api/nurses/me/patients` | H | H | NUR |
| DELETE | `/api/care-assignments/{id}` | N | E | ROLE |
| POST | `/api/doctors/{doctorProfileId}/availability` | H | B | DOC, ROLE |
| GET | `/api/doctors/{doctorProfileId}/availability` | N | N | Remaining |
| DELETE | `/api/doctor-availability/{id}` | N | E | ROLE |
| GET | `/api/patients/{patientId}/alerts` | H | B | ALT, OWN |
| GET | `/api/alerts/{id}` | N | N | Remaining |
| PATCH | `/api/alerts/{id}/acknowledge` | H | B | ALT, ROLE |
| PATCH | `/api/alerts/{id}/resolve` | H | B | ALT, ROLE |

| Coverage status | Before | After |
| --- | --- | --- |
| Both | 6 | 18 |
| Happy only | 9 | 1 |
| Error only | 0 | 12 |
| Neither | 24 | 8 |
| Total endpoints | 39 | 39 |

After Day 1, **21 endpoints still lack one or both required HTTP paths**. Closing the five selected priority gap groups is not a claim of complete API coverage.

## Task 3 — Prioritize Gaps by Risk

| Rank | Gap group | Risk | Reason for priority |
| --- | --- | --- | --- |
| 1 | Authentication error paths | P0 | Invalid credentials or duplicate accounts must not create unsafe or inconsistent identities. |
| 2 | Refresh-token rotation and replay | P0 | Reusable consumed refresh tokens could enable unauthorized continued access. |
| 3 | Cross-Patient clinical reads | P0 | Missing resource checks can expose another Patient's health information. |
| 4 | Privileged write authorization | P0 | Incorrect role checks can allow Patient accounts to mutate clinical data, staff identities, or schedules. |
| 5 | Catalog query-budget regression | P1 | An N+1 regression can restore database load and erase Sprint 3's optimization. |

This application has no payment, checkout, stock, or cart routes. The curriculum's commerce examples are therefore not invented as features here. The corresponding project-specific risks include appointment conflicts, clinical-data access, care-assignment changes, and medical-alert transitions.

### Next highest-risk backlog

1. Test individual vital-sign, medication, appointment, and alert reads for valid access, missing IDs, and cross-Patient access.
2. Add allowed-role happy paths for the 12 write endpoints currently covered only by rejection tests; also exercise missing resources and business-rule failures.
3. Cover duplicate and concurrent appointment booking using SQL Server, including the unique-index conflict path.
4. Cover Nurse assignment revocation, Doctor schedule ownership, and repeated or invalid alert transitions.
5. Complete staff lists, assignment lists, availability reads, and the missing Nurse-list error case.

## Task 4 — Close Five Priority Gap Groups

### 1. Authentication failures

`Login_RejectsInvalidCredentials` tests both an unknown email and the wrong password for a genuinely registered account. Both receive `401 Unauthorized` with the same safe message, `Invalid email or password.`

`Register_RejectsDuplicateEmail_WithoutCreatingAnotherProfile` registers twice with the same email, expects `400 Bad Request`, and queries the test database to confirm that the original user still has exactly one Patient profile.

### 2. Refresh-token lifecycle

The refresh test registers and logs in through the real API, obtains a persisted token pair, and verifies:

- A matching expired access token and unused refresh token produce a replacement pair.
- The replacement refresh token differs from the original.
- The original persisted refresh token is marked used.
- Replaying the original pair returns `400 Bad Request`.
- The newly issued refresh token remains unused.

To exercise expiry without a time-based wait, the test re-signs the original access token's claims with a past expiration using the configured test signing key. This preserves its identity and JTI. This helper is test-only; it is not an application token-generation feature.

A separate malformed-access-token test checks a safe `400` response rather than an unhandled exception.

### 3. Patient resource ownership

The ownership theory independently exercises six Patient read routes: profile, clinical details, vital signs, medications, appointments, and alerts. A valid Patient token receives `200 OK` for Patient 1, which is linked to its identity, and `403 Forbidden` for Patient 2.

For clinical details, the test also verifies that the returned profile and all included vital signs belong to Patient 1. The medications, appointments, and alerts happy paths use empty seeded collections; populated response-mapping tests remain useful follow-up work.

### 4. Privileged write roles

The role theory sends requests through the real authentication and authorization middleware for 19 protected POST, PUT, DELETE, and PATCH routes. A Patient token must receive `403`, even when the body is an empty object or the referenced resource is missing.

The requests intentionally use minimal bodies: authorization should reject the forbidden role before model validation or action execution. The test additionally asserts **zero database queries**, providing evidence that forbidden operations never reach database-backed services.

These cases verify one denied role. They do not replace allowed-role tests or a complete Admin, Doctor, Nurse, and Patient permission matrix.

### 5. N+1 regression safeguard

```text
Admin GET /api/patients
    -> JWT and role authorization
    -> PatientService DTO projection
    -> SQL COUNT + paginated SELECT
    -> 200 response with a test-only SQL command count

Page size 1  -> 2 SQL commands
Page size 50 -> 2 SQL commands
```

The test host attaches the existing `DatabaseQueryCounterInterceptor` to SQLite. `QueryCountStartupFilter` exposes the completed request's count through `X-Test-Database-Query-Count` when the response starts. The filter and header are registered only by `CardiacMonitorApiFactory`, not by the production application.

The regression asserts the actual query count, response status, item count, and total count. It protects query structure, not latency or SQL Server execution-plan quality.

## Task 5 — Run the Full Test Suite

Verification was performed on **September 17, 2026** from the CardiacMonitor project directory.

```powershell
dotnet test CardiacMonitor.slnx --no-restore --verbosity minimal
```

This command builds and runs both test projects using the already restored dependencies. On a fresh checkout, restore first:

```powershell
dotnet restore CardiacMonitor.slnx
dotnet test CardiacMonitor.slnx --no-restore --verbosity minimal
```

To run only the new Day 1 cases:

```powershell
dotnet test tests/CardiacMonitor.IntegrationTests/CardiacMonitor.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~Sprint4Day1Tests
```

### Measured results

| Suite | Before | After | Failed | Skipped |
| --- | --- | --- | --- | --- |
| Unit tests | 34 | 34 | 0 | 0 |
| Integration tests | 18 | 49 | 0 | 0 |
| Total | 52 | 83 | 0 | 0 |

The 31 new cases consist of two login cases, one duplicate-registration case, two refresh cases, six ownership cases, 19 privileged-write cases, and one query-budget case. The query-budget case performs two separate HTTP requests.

### Test environment and limitations

- xUnit runs the tests; the existing unit suite uses Moq for isolated controller/service collaborators.
- `WebApplicationFactory<Program>` exercises the real ASP.NET Core request pipeline and application services.
- A fresh SQLite in-memory database is created for each new test case, keeping data and rate-limit state isolated.
- Signed test JWTs exercise JWT bearer validation and role authorization. Authentication lifecycle tests additionally use real registration and login.
- The `Testing` environment uses distributed memory caching instead of requiring a Redis server.
- SQLite verifies relational queries and application behavior, but does not prove SQL Server-specific execution plans, concurrent booking behavior, or production Redis connectivity.
- No code-coverage percentage, production benchmark, deployment, or mentor review is claimed by these results.

## Files Added or Updated

| File | Purpose |
| --- | --- |
| `tests/CardiacMonitor.IntegrationTests/Sprint4Day1Tests.cs` | Tests closing the five selected priority gap groups. |
| `tests/CardiacMonitor.IntegrationTests/QueryCountStartupFilter.cs` | Test-only response header for request-local SQL counts. |
| `tests/CardiacMonitor.IntegrationTests/CardiacMonitorApiFactory.cs` | Registers the test observer and attaches the SQL interceptor to SQLite. |
| `REAMDE01.md` | Sprint goal, backlog, complete endpoint audit, risk ranking, implementation evidence, and verification results. |

Existing application changes in `Program.cs` were preserved. This Day 1 work does not change production endpoint behavior or add a production diagnostics header.

## Day 1 Result

Sprint 4 now has an explicit goal and risk-ranked backlog, a complete 39-endpoint coverage audit, five addressed priority gap groups, and a passing 83-test suite. The Sprint 3 retrospective action is implemented as a real endpoint regression test with the correct two-query budget. Remaining gaps are visible and ready for the next Sprint 4 testing tasks.
