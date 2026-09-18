# Cardiac Monitor API — Week 9 Day 5

## Definition of Done Audit, Sprint Review and Retrospective

Sprint 4 brings CardiacMonitor together as a tested and documented capstone API. The Day 5 audit confirms a clean Release build, **94 passing automated tests**, a **39-operation Postman collection**, and a complete local setup guide. The database documentation was refreshed to reflect the current care-team, scheduling, and medical-alert features.

**Close-out position:** local verification is complete for the checks recorded below. The project owner confirms CI/CD preparation is ready. **Public deployment and its live verification are the remaining release milestone.** This summary does not claim that the full Definition of Done has already been achieved.

Audit date: **September 18, 2026**.

## Sprint 4 Highlights

| Area | Achievement |
| --- | --- |
| API functionality | Patient profiles, vital signs, medications, appointments, staff, care assignments, Doctor availability, and medical alerts |
| Security | Identity/JWT authentication, refresh-token rotation, role checks, and Patient-resource access control |
| Automated verification | 34 unit tests and 60 integration tests; no failures or skipped cases |
| Performance protection | A regression test keeps the Patient catalog at two SQL queries as page size grows |
| API documentation | Swagger XML comments, realistic example pairs, and Postman coverage for all 39 API operations |
| Database | Six EF Core migrations and an updated current-schema ERD |
| Developer onboarding | Setup, SQL Server/Redis prerequisites, environment variables, authentication, and API documentation links |
| Delivery | CI/CD readiness confirmed by the project owner; public deployment is next |

The sprint strengthens the existing implementation without adding unrelated features or changing established API behavior.

## Task 1 — Run the Definition of Done Audit

### Audit approach

The local audit used a fresh, non-incremental Release build, the complete automated test suite, the documentation regression tests, the current EF Core model/configurations, migration files, and the project README.

Status labels describe the evidence actually available:

- **PASS — local:** the check was verified against local code, files, or a completed test run.
- **OWNER CONFIRMED:** readiness was reported by the project owner; remote run history was not independently retrieved.
- **FAIL — pending release:** the final live-deployment requirement has not yet been completed.

### Checklist results

| Definition of Done item | Result | Evidence and close-out interpretation |
| --- | --- | --- |
| All sprint tasks complete and API runs end-to-end without critical errors | PASS — local API verification; release sign-off pending | All 94 tests pass, including HTTP integration checks. Formal all-tasks-complete sign-off follows deployment; an external sprint board was not inspected. |
| REST API documented through Swagger/OpenAPI and Postman, with at least one test per endpoint | PASS — local | Documentation tests verify all 39 unique operations have matching Postman requests, expected-status scripts, and saved examples. |
| Database schema documented through an ERD and managed with EF Core migrations | PASS — local, after documentation refresh | Current ERD covers all ten application entities plus essential Identity relationships. Six migrations are present. |
| GitHub project has a complete README covering setup, stack, environment variables, and API docs | PASS — local README content | [README.md](README.md) contains the required setup and configuration sections and documentation links. Push is needed to publish local changes to GitHub. |
| Deployed on Azure App Service or Railway with a live URL and passing CI/CD | FAIL — pending release | CI/CD readiness is owner-confirmed. A live CardiacMonitor URL and a successful deployment run remain to be recorded. |
| Code builds with zero compiler warnings; nullable warnings resolved | PASS — local | Non-incremental Release build completed with 0 warnings and 0 errors; the full test run also completed successfully. |

### Verification commands

Run from the CardiacMonitor directory:

```powershell
dotnet build CardiacMonitor.csproj --configuration Release --no-incremental --verbosity minimal
dotnet test CardiacMonitor.slnx --configuration Release --verbosity minimal
```

### Actual results

| Check | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| Unit tests | 34 | 0 | 0 |
| Integration tests | 60 | 0 | 0 |
| Total | **94** | **0** | **0** |

| Release build | Result |
| --- | --- |
| Compiler warnings | **0** |
| Compiler errors | **0** |
| Exit code | **0** |

The Release build was not made green by adding warning suppressions or changing test expectations.

### What the test evidence covers

- Authentication failures and duplicate registration.
- Refresh-token rotation and replay protection.
- Patient ownership and privileged-write authorization.
- Clinical and care-team API scenarios in the existing suite.
- The Patient catalog query-budget regression.
- XML documentation on seven important operations.
- Request/response examples on three operations.
- Complete Swagger/Postman parity across 39 operations.

Integration tests run through WebApplicationFactory with an isolated SQLite database and distributed memory caching. They verify the application pipeline; live SQL Server, Redis, and hosting connectivity are checked during deployment.

The Postman audit verifies the presence of test scripts for every operation. It is not presented as a completed Postman runner session against a public deployment, nor is the number of tests a claim of 100% code coverage.

## Task 2 — Close the Gaps Found by the Audit

### Database documentation refresh — completed

The earlier ERD explained the core Patient, vital-sign, medication, and appointment relationships. Day 5 expanded the current schema documentation to include:

- Doctor and Nurse professional profiles.
- Patient care assignments and the assigning identity.
- Doctor weekly availability.
- Medical alerts, optional vital-sign linkage, and acknowledgement/resolution users.
- Refresh-token ownership.
- Identity role membership.
- Current deletion rules, unique indexes, and integrity constraints.

Updated files:

| File | Purpose |
| --- | --- |
| [docs/ERD.md](docs/ERD.md) | Current rendered Mermaid diagram, relationship summary, constraints, and migration guidance |
| [docs/CardiacMonitor-ERD-Current.mmd](docs/CardiacMonitor-ERD-Current.mmd) | Editable current-schema diagram source |
| [README05.md](README05.md) | Day 5 audit, Sprint 4 summary, review, retrospective, and release handoff |

The earlier Chen image and core-domain Mermaid source remain available as historical presentation assets. No entity, migration, database, controller, service, or business rule was changed for this documentation refresh.

### Final release milestone — deployment

The project owner confirms pipeline readiness. The remaining work is to connect that preparation to a live application and verify the result:

1. Deploy CardiacMonitor with production SQL Server and Redis configuration.
2. Store database, Redis, and JWT secrets in hosting environment variables, not Git.
3. Apply the existing migrations to the intended database after reviewing its configuration and seed data.
4. Confirm login and a protected API request at the live HTTPS URL using synthetic data.
5. Verify Doctor availability against the configured Redis service.
6. Confirm a successful main-branch pipeline run includes deployment after all tests pass.
7. Record the live URL and successful run link, then complete the final sprint sign-off.

If MonsterASP is selected, obtain mentor approval first: the supplied Definition of Done explicitly names Azure App Service or Railway. MonsterASP can demonstrate the same delivery principles, but acceptance of the substitute platform is a mentor decision.

Use only disposable demonstration data. Development seed accounts and checked-in development secrets are not production credentials.

## Task 3 — Re-check the Corrected Items

The ERD documentation was checked again against:

- The ten application entities exposed through AppDbContext.
- The relationship and deletion rules in Data/Configurations.
- The physical table names in AppDbContextModelSnapshot.
- The current six-file migration history.

The review confirms that:

- Appointment.DoctorId references an Identity user ID.
- DoctorAvailability.DoctorProfileId references the numeric Doctor profile ID.
- PatientCareAssignment links Patient, NurseProfile, and assigning Identity user.
- MedicalAlert links Patient, an optional VitalSign, and optional audit identities.
- The refreshed diagram distinguishes clinical data, professional profiles, and Identity ownership.

The documentation-only correction does not require a schema migration. The completed build and full test run above establish the application baseline; no production code was altered afterward.

**Deployment re-check:** complete the live smoke checks and successful deployment-run verification after publishing. That is the final gate before changing the overall Definition of Done to complete.

## Task 4 — Sprint 4 Review and Retrospective

### Sprint goal

Deliver a capstone API with dependable automated verification, usable API documentation, clear setup instructions, and a safety-gated release process.

### Sprint summary

| Sprint area | Outcome |
| --- | --- |
| Planning and coverage audit | Endpoint inventory and risk-based priorities documented in [REAMDE01.md](REAMDE01.md) |
| Critical regression protection | Authentication, resource isolation, privileged writes, and catalog query behavior covered |
| Documentation | Swagger, Postman, and onboarding completed in [REAMDE02.md](REAMDE02.md) and the main README |
| Continuous delivery preparation | Ready according to the project owner |
| Definition of Done audit | Local checks passed; current database documentation refreshed |
| Presentation readiness | Review/demo sequence prepared; live deployment is the final release step |

The Sprint 3 action to protect the N+1 optimization is carried forward through the catalog query-budget test. The documented contract is two queries: a count and a projected page query.

### Sprint Review — demonstration outline

This is the written review package, not a claim that a mentor review meeting has already occurred.

1. Explain the cardiac-care domain and the Controller → Service → DTO → EF Core structure.
2. Demonstrate Patient registration and login.
3. Use role-appropriate credentials to show a clinical workflow and Patient-resource protection.
4. Demonstrate care assignment or Doctor availability.
5. Show medical-alert acknowledgement/resolution behavior.
6. Open Swagger, the 39-operation Postman collection, and the refreshed ERD.
7. Present the zero-warning Release build and 94 passing tests.
8. After deployment, demonstrate the live URL and the successful build/test/deploy run.
9. Present the completed audit and the retrospective action for Week 10.

Prepare synthetic users and records before the demonstration. Do not depend on creating every prerequisite during the presentation.

### Retrospective — what went well

- Separating HTTP handling, business services, and DTO contracts kept the API understandable.
- Risk-based testing focused effort on authentication, authorization, and resource ownership.
- The query-budget test converted a performance improvement into durable regression protection.
- Swagger examples and complete Postman coverage made the API easier to demonstrate and consume.
- Local setup and environment-variable guidance reduced onboarding ambiguity.
- The final audit brought the database documentation into line with the implemented features.

### Retrospective — lessons to carry forward

- A green test suite is strongest when each test protects a clear behavior.
- Documentation belongs beside the evolving implementation, especially when the database gains new features.
- Passing tests should be the deployment gate, not a separate optional check.
- A focused presentation is more convincing than walking through every source file.

### One action for Week 10

**Rehearse a 7-minute demonstration twice using the same synthetic dataset.**

Owner: Mohammad Abdelfattah.

Acceptance criteria:

- Explain the problem, architecture, and security model clearly.
- Demonstrate one complete clinical workflow.
- Show the live API, documentation, ERD, and passing pipeline.
- Finish within seven minutes.
- Keep a short backup demonstration ready in case the network is unavailable.

### Release evidence to record after deployment

| Evidence | Value |
| --- | --- |
| Live CardiacMonitor HTTPS URL | Add after deployment |
| Successful main-branch build/test/deploy run | Add after verification |
| Hosting-platform acceptance | Confirm with mentor if using MonsterASP |
| Live login/protected request/Redis smoke checks | Record after execution |
| Final Definition of Done sign-off | Complete after the release evidence above |

## Day 5 Result

CardiacMonitor has a clean Release build, **94 passing automated tests**, documented contracts for **39 API operations**, complete setup guidance, and a refreshed ERD for the current schema. Sprint 4's review package and retrospective are prepared. **Public deployment is the final milestone before full release sign-off.**
