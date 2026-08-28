# Day 5 - Sprint Review, Postman Demo, and Retrospective

## Objective

Day 5 closes Sprint 1. It does not introduce another large API feature. Its purpose is to prove that the completed work functions end to end, evaluate each backlog item against the Definition of Done, record unfinished work, and identify one measurable improvement for Sprint 2.

## Day 5 deliverables

| Deliverable | Repository evidence |
| --- | --- |
| Repeatable Postman demo | `Sprint1_Day5_Demo.postman_collection.json` |
| Filtered and paginated catalog demonstration | Request 02 in the Day 5 collection |
| Complete order and visible stock decrement | Requests 03 through 06 in the Day 5 collection |
| Business-rule error demonstration | Request 07 verifies insufficient stock returns `409 Conflict` |
| Happy-path and error-path API tests | `CardiacMonitor.Tests/Integration/SprintOneApiTests.cs` |
| Updated ERD | `docs/CardiacMonitor-ERD-Chen-v2.png` |
| Sprint Review, migration history, backlog, and retrospective | `WEEK6_SPRINT1_SUMMARY.md` |

## Prerequisites

1. Install the .NET 8 SDK and SQL Server.
2. Configure `ConnectionStrings:CardiacMonitorConnection` for the local SQL Server instance.
3. Apply all migrations.
4. Start the API with its HTTPS profile.
5. Import the Day 5 collection into Postman.

```powershell
dotnet restore
dotnet ef database update
dotnet run --launch-profile https
```

Default demo URL:

```text
https://localhost:7142
```

If Postman rejects the local ASP.NET Core development certificate, disable SSL certificate verification only for this local demo or trust the development certificate with `dotnet dev-certs https --trust`.

## Postman demo

Import:

```text
Sprint1_Day5_Demo.postman_collection.json
```

Run all requests in their numbered order. Collection variables carry the JWT, medication ID, and stock values between requests automatically.

### 01 - Login as seeded doctor

- Sends the development doctor credentials.
- Expects `200 OK`.
- Saves the returned JWT as `accessToken`.

### 02 - Browse the catalog

Calls:

```http
GET /api/patients?pageNumber=1&pageSize=1&searchName=a&gender=Female&sortBy=lastName&isDescending=true
```

This proves pagination, two optional filters, sorting, authorization, and DTO projection in one request.

### 03 - Create demo medication

- Creates an active medication with five units of stock and a unit price of `3.50`.
- Saves the generated medication ID and starting stock.

### 04 - Verify stock before ordering

- Reads the medication.
- Confirms that the starting stock is five.

### 05 - Create the order

- Orders two units.
- Expects `201 Created`.
- Confirms a line total and order total of `7.00`.

### 06 - Verify the stock decrement

- Reads the same medication again.
- Confirms that the stock changed from five to three.

### 07 - Demonstrate the error path

- Attempts to order more stock than is available.
- Expects `409 Conflict` and an explanatory error message.

## Automated acceptance checks

Run:

```powershell
dotnet test .\CardiacMonitor.slnx --configuration Release
```

The current suite contains 27 passing tests. Day 5 adds HTTP-level coverage for:

- Authorized catalog pagination, filtering, sorting, and projection.
- Unauthorized catalog access.
- Successful order creation, totals, persistence, and stock decrement.
- Insufficient-stock rejection with no order rows or stock changes.

Verify that the EF Core model and migration snapshot remain synchronized:

```powershell
dotnet ef migrations has-pending-model-changes --project .\CardiacMonitor.csproj --no-build
```

Expected output:

```text
No changes have been made to the model since the last migration.
```

## Sprint Review

A backlog item is counted as Done only when:

- The endpoint returns the correct status codes.
- A happy path and an error path are covered by passing tests.
- No unhandled exception leaks implementation details.
- Known security issues are recorded and assigned.
- The pull request has been reviewed and approved.

Technical implementation is ready for review. The live Postman run, SQL Server evidence, mentor approval, and merged pull-request link remain external actions and are intentionally marked as pending.

## Sprint 2 backlog

| Priority | Item | Acceptance evidence |
| --- | --- | --- |
| P0 | Open, review, and merge the Sprint 1 pull request | Approved PR, green checks, and merged URL |
| P0 | Move JWT signing material out of tracked configuration | User Secrets/environment/secret manager supplies the key |
| P0 | Prevent public registration from selecting privileged roles | Public registration cannot create an Admin or Doctor |
| P1 | Run the seven-request demo against SQL Server | All Postman tests pass and stock changes are visible in the database |
| P1 | Add a relational last-unit concurrency test | Only one competing order succeeds and stock never becomes negative |
| P2 | Document or reconcile historical migration naming | A clean database can be created from the reviewed migration chain |

## Sprint 1 Retrospective

### What went well

- Controllers remain focused on HTTP behavior while services own business logic.
- Read endpoints use DTO projection, pagination, filtering, and sorting.
- The order operation keeps stock changes and inserted rows consistent.
- Tests cover both expected success and business-rule failure behavior.
- The Postman collection is now executable in a deterministic order.

### What should improve

- Postman examples and close-out evidence should be maintained throughout the sprint rather than assembled on the final day.
- Security debt must be assigned explicitly instead of remaining only as comments or README warnings.
- SQL Server concurrency behavior requires relational testing; EF Core InMemory tests cannot prove isolation semantics.
- Pull-request review should begin early enough to address mentor feedback before Sprint Review.

### Concrete action for Sprint 2

Before implementing each Sprint 2 endpoint, create its Postman request and its happy-path and error-path acceptance tests. A backlog card cannot move to Done until the endpoint, tests, and Postman example all pass.

## ERD

![Cardiac Monitor Chen-style ERD](../CardiacMonitor-ERD-Chen-v2.png)

The presentation ERD includes the clinical domain, medication stock and pricing, medication orders, and order items. Identity role and refresh-token tables are intentionally omitted because they are infrastructure rather than the core business-domain view.

## Mentor handoff checklist

- [x] Source code builds successfully.
- [x] Automated test suite passes locally.
- [x] EF Core reports no pending model changes.
- [x] Day 5 Postman collection is valid and ordered for demonstration.
- [x] ERD reflects medication orders, order items, stock, and price.
- [x] Sprint Review, backlog, and retrospective are documented.
- [ ] Run the Postman collection live against SQL Server.
- [ ] Show the medication row before and after the successful order.
- [ ] Add the approved and merged pull-request URL to `WEEK6_SPRINT1_SUMMARY.md`.

## Related documentation

- [Sprint 1 close-out summary](../../WEEK6_SPRINT1_SUMMARY.md)
- [Main project README](../../README.md)
- [Day 4 implementation notes](../day04/README.md)
