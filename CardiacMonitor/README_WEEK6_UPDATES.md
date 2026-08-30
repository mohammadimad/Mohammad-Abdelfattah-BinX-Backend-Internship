# Cardiac Monitor API - Week 6 Improvements

This document describes the database, query, business-logic, testing, and documentation improvements added to the Cardiac Monitor API for Week 6.

The update follows the Week 6 topics: explicit EF Core modeling, deterministic seed data, code-first migrations, paginated and filterable read routes, transactional write operations, testing, and sprint documentation.

## What Changed

- Split the EF Core Fluent API configuration into one class per entity.
- Replaced changing seed values with deterministic values.
- Added database length limits, check constraints, and optimized indexes.
- Added the `Week6DataModelImprovements` migration.
- Added pagination, filtering, and sorting to patient vital-sign history.
- Added a reusable `PagedResult<T>` response contract.
- Made user registration atomic with a database transaction.
- Made refresh-token rotation atomic with a database transaction.
- Prevented a doctor from being booked twice at the exact same time.
- Added unit and integration tests for the new behavior.
- Updated the ERD and Postman collection.

## Updated Project Areas

```text
Data/
|- AppDbContext.cs
|- SeedData.cs
|- Configurations/
|  |- AppointmentConfiguration.cs
|  |- MedicationConfiguration.cs
|  |- PatientConfiguration.cs
|  |- RefreshTokenConfiguration.cs
|  `- VitalSignConfiguration.cs
`- Migrations/
   `- *_Week6DataModelImprovements.cs

DTOs/
`- VitalSignDtos.cs

Services/
|- AuthServices.cs
|- AppointmentService.cs
|- IVitalSignService.cs
`- VitalSignService.cs

Validators/
`- VitalSignQueryParametersValidator.cs

tests/
|- CardiacMonitor.UnitTests/
`- CardiacMonitor.IntegrationTests/
```

## EF Core Configuration Structure

`AppDbContext` now applies every entity configuration from the project assembly:

```csharp
// Configures the database model and applies deterministic seed data.
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    SeedData.Configure(builder);
}
```

Each entity has a focused configuration class implementing `IEntityTypeConfiguration<T>`. This keeps `AppDbContext` small and makes relationships, indexes, and constraints easier to review.

## Deterministic Seed Data

The old model generated values with `DateTime.UtcNow`, `Guid.NewGuid()`, and `PasswordHasher` while EF Core was building the model. Those values changed on every design-time run, so EF Core continuously reported pending model changes.

`SeedData` now uses fixed timestamps, security stamps, and an existing fixed development password hash. As a result, this command reports a clean model after the migration is created:

```powershell
dotnet ef migrations has-pending-model-changes --project CardiacMonitor.csproj
```

Expected result:

```text
No changes have been made to the model since the last migration.
```

> The seeded doctor account is demonstration data. Remove it or replace its credentials before a production deployment.

## Database Constraints and Indexes

The migration adds the following protections and query optimizations.

### Field limits

- Patient first and last names: 100 characters.
- Patient gender: 20 characters.
- Patient contact number: 30 characters.
- Medication name: 150 characters.
- Medication dosage and frequency: 100 characters.
- Appointment status: 30 characters.
- Appointment notes: 1,000 characters.
- Refresh token: 256 characters.
- Refresh-token JWT identifier: 100 characters.

### Check constraints

- Heart rate: 30-250 bpm.
- Oxygen saturation: 50-100%.
- Systolic blood pressure: 70-220 mmHg.
- Diastolic blood pressure: 40-130 mmHg.
- Appointment status: `Scheduled`, `Completed`, or `Cancelled`.

These database constraints complement FluentValidation. Validation gives clients a friendly HTTP 400 response, while database constraints protect integrity if data is written through another path.

### Indexes

| Index | Purpose |
| --- | --- |
| `IX_VitalSigns_PatientId_RecordedAt` | Speeds up a patient's chronological vital-sign history. |
| `IX_Medications_PatientId_IsActive` | Speeds up active-medication queries. |
| `IX_Appointments_PatientId_AppointmentDate` | Speeds up a patient's appointment timeline. |
| `UX_Appointments_DoctorId_AppointmentDate` | Prevents an exact doctor time conflict. |
| `UX_RefreshTokens_Token` | Guarantees token uniqueness and speeds up refresh lookup. |
| `IX_RefreshTokens_ExpiryDate` | Supports expired-token cleanup. |

## Applying the Migration

Back up important development data before applying a schema-changing migration. Existing rows must satisfy the new length and check constraints.

```powershell
dotnet ef database update
```

The migration is:

```text
Week6DataModelImprovements
```

It creates the `RefreshTokens` table because the previous migration named `AddRefreshTokenTable` did not contain the table-creation operation in its generated `Up` method.

## Paginated Vital-Sign History

The endpoint remains:

```http
GET /api/patients/{patientId}/vitals
```

It now accepts these optional query parameters:

| Parameter | Default | Description |
| --- | ---: | --- |
| `page` | 1 | Requested page number. |
| `pageSize` | 20 | Items per page; allowed range is 1-100. |
| `from` | null | Includes records at or after this UTC date/time. |
| `to` | null | Includes records at or before this UTC date/time. |
| `minHeartRate` | null | Minimum heart rate, from 30 to 250. |
| `maxHeartRate` | null | Maximum heart rate, from 30 to 250. |
| `sort` | `recordedAt_desc` | Requested supported sorting mode. |

Supported sort values:

- `recordedAt_desc`
- `recordedAt_asc`
- `heartRate_desc`
- `heartRate_asc`

Example:

```http
GET /api/patients/1/vitals?page=1&pageSize=10&minHeartRate=60&maxHeartRate=120&sort=heartRate_desc
Authorization: Bearer ACCESS_TOKEN
```

The query applies filters before counting, projects directly to `VitalSignResponse`, and then uses `Skip` and `Take`:

```csharp
var totalCount = await query.CountAsync();
var orderedQuery = ApplySorting(query, queryParameters.Sort);
var items = await orderedQuery
    .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
    .Take(queryParameters.PageSize)
    .Select(vital => new VitalSignResponse(/* selected fields */))
    .ToListAsync();
```

### Response contract

```json
{
  "items": [
    {
      "id": 2,
      "patientId": 1,
      "heartRate": 82,
      "oxygenSaturation": 97.0,
      "systolicBP": 125,
      "diastolicBP": 82,
      "recordedAt": "2026-08-15T16:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

### Breaking API contract change

This endpoint previously returned a JSON array. It now returns a paginated object containing `items` and page metadata. Any frontend consuming the endpoint must read `response.items` instead of treating the response itself as the array.

## Query Validation

`VitalSignQueryParametersValidator` rejects:

- Page numbers below 1.
- Page sizes outside 1-100.
- A `from` value later than `to`.
- Heart-rate filters outside the medically supported input range.
- A minimum heart rate greater than the maximum.
- Unknown sort values.

Invalid queries return standard `ValidationProblemDetails` with HTTP 400.

## Transactional User Registration

The old registration flow created the user before confirming that the requested role existed. An invalid role could therefore leave an identity user without any role.

The new order is:

1. Check whether the email already exists.
2. Verify that the requested role exists.
3. Begin a database transaction.
4. Create the Identity user.
5. Assign the role.
6. Commit only after both writes succeed.
7. Roll back if creation, assignment, or an unexpected operation fails.

```csharp
await using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    var creationResult = await _userManager.CreateAsync(user, request.Password);
    var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Atomic Refresh-Token Rotation

The previous refresh flow saved `IsUsed = true` on the old token before generating and saving the replacement. If the second save failed, the user could lose the old token without receiving a new one.

The updated flow:

1. Validates the expired access token and stored refresh token.
2. Starts a transaction.
3. Marks the old refresh token as used.
4. Adds the new refresh token without saving separately.
5. Saves both changes together.
6. Commits the transaction.

This provides all-or-nothing token rotation.

## Appointment Scheduling Conflict

`AppointmentService` now checks whether the selected doctor already has an appointment at the exact requested date and time. The database also enforces the same rule with a unique composite index.

This uses two layers:

- Service validation returns a controlled failure before attempting the insert.
- The unique index protects against race conditions where two requests pass the service check concurrently.

## Tests

The updated test suite contains:

- 20 unit tests.
- 11 integration tests.
- 31 total passing tests.

New coverage includes:

- Vital-sign pagination, filtering, and sorting.
- Query-parameter validation.
- Invalid-role registration without an orphan user.
- Successful transactional registration with role membership.
- Doctor scheduling conflicts.
- SQLite compatibility for database constraints.

Run all checks:

```powershell
dotnet build CardiacMonitor.slnx --no-restore
dotnet test CardiacMonitor.slnx --no-build --no-restore
dotnet ef migrations has-pending-model-changes --project CardiacMonitor.csproj --no-build
```

## Postman Demo

Import:

```text
Cardiac Patient Monitoring API.postman_collection.json
```

Set the collection variables:

- `baseUrl`: `https://localhost:7142`
- `accessToken`: the JWT returned by login

Run `GET Paginated and Filtered Patient Vital Signs` and change its query parameters live to demonstrate pagination, filtering, and sorting.

## ERD

The presentation ERD remains intentionally focused on the medical domain. It omits `AspNetRoles`, `AspNetUserRoles`, and `RefreshTokens`, as requested. `AspNetUsers` represents authenticated identities, and a user carrying the Doctor role is referenced through `Appointments.DoctorId`.

See `docs/ERD.md` and `docs/CardiacMonitor-ERD-Chen.png`.

## New Method Comment Convention

Every newly introduced method includes a short English `//` comment immediately before it, for example:

```csharp
// Applies a supported deterministic sort order to a vital-sign query.
private static IOrderedQueryable<VitalSign> ApplySorting(...)
```

The comments state responsibility without repeating the implementation line by line.

## Final Result

The API now meets the main Week 6 technical outcomes: explicit and reviewable database configuration, stable migrations, indexed and constrained data, a paginated and filterable read route, real transaction boundaries, updated tests, and demo-ready documentation.
