# Week 5 - Day 5: Applying Testing and Week Synthesis

## Day Overview

Day 5 applied the week's testing techniques to the highest-risk parts of the Cardiac Patient Monitoring API. The focus was on meaningful risk coverage rather than testing every trivial property or chasing 100% coverage.

## What We Learned

- How to prioritize tests by business risk and complexity.
- Why authentication, authorization, and clinical validation require early coverage.
- How unit and integration tests complement each other.
- How to run the complete suite with one `dotnet test` command.
- How this testing foundation carries into the Phase 3 sprint structure.

## Highest-Risk Areas Selected

1. Authentication and refresh-token replay prevention.
2. Patient ownership authorization.
3. Clinical vital-sign validation.

## Tasks We Completed

### Task 1: Test refresh-token replay prevention

The first refresh succeeds, while a second attempt with the same refresh token is rejected.

```csharp
var firstRefresh = await service.RefreshTokenAsync(refreshRequest);
var reusedRefresh = await service.RefreshTokenAsync(refreshRequest);

Assert.True(firstRefresh.IsSuccess);
Assert.False(reusedRefresh.IsSuccess);
Assert.Equal("Refresh token has already been used.", reusedRefresh.Message);
```

### Task 2: Test patient ownership

A patient attempting to read another patient's profile receives `403 Forbidden`.

```csharp
SetAuthenticatedUser("different-user-id", "Patient");

var result = await _controller.GetById(patientId);

Assert.IsType<ForbidResult>(result);
```

### Task 3: Test clinical validation

The validator rejects heart rate, oxygen saturation, and blood-pressure values outside the accepted ranges.

```csharp
var request = new CreateVitalSignRequest(
    HeartRate: 29,
    OxygenSaturation: 49.9m,
    SystolicBP: 221,
    DiastolicBP: 39);

var result = _validator.Validate(request);

Assert.False(result.IsValid);
```

### Task 4: Keep important integration coverage

The primary patient endpoint has authenticated happy-path, not-found, and unauthorized integration tests. Global exception handling is also verified through the real in-memory HTTP pipeline.

### Task 5: Run the complete test suite

Run the following command from the `CardiacMonitor` directory:

```powershell
dotnet test .\CardiacMonitor.slnx --configuration Release
```

Latest verified result:

```text
Passed: 21
Failed: 0
Skipped: 0
```

## Files Related to Day 5

- `CardiacMonitor.Tests/AuthServiceTests.cs`
- `CardiacMonitor.Tests/PatientsControllerTests.cs`
- `CardiacMonitor.Tests/CreateVitalSignRequestValidatorTests.cs`
- `CardiacMonitor.Tests/PatientServiceTests.cs`
- `CardiacMonitor.Tests/Integration/PatientsApiTests.cs`
- `CardiacMonitor.Tests/Integration/GlobalExceptionMiddlewareTests.cs`
- `WEEK5_SUMMARY.md`

## Week 5 Final Result

The project now contains xUnit unit tests, Moq-isolated service tests, WebApplicationFactory integration tests, an isolated InMemory test database, authenticated endpoint coverage, centralized ProblemDetails error handling, structured logging, and a full passing test suite.
