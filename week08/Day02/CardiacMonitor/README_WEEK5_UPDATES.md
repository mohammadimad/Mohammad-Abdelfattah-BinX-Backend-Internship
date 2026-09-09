# Testing, Error Handling, and Reliability Enhancements

This document explains the reliability and security improvements added to the Cardiac Monitor API. It focuses on centralized exception handling, standardized API errors, structured logging, doctor-role validation, unit testing, and integration testing.

## What Was Added

The project now includes:

- Centralized exception handling with `IExceptionHandler`.
- Safe and consistent error responses using `ProblemDetails`.
- Standardized `401`, `403`, `404`, `429`, and `500` responses.
- Structured logging for unexpected exceptions and authentication failures.
- Doctor-role verification before creating or updating appointments.
- Unit tests using xUnit, Moq, and Arrange-Act-Assert.
- Integration tests using `WebApplicationFactory<Program>`.
- A relational SQLite in-memory database for isolated integration tests.
- Real signed JWTs for authenticated integration tests.
- A single solution-level test command.

## Updated Project Structure

```text
CardiacMonitor/
├── Infrastructure/
│   ├── GlobalExceptionHandler.cs
│   └── ProblemDetailsAuthorizationMiddlewareResultHandler.cs
├── tests/
│   ├── CardiacMonitor.UnitTests/
│   │   ├── Controllers/
│   │   ├── Services/
│   │   └── Validators/
│   └── CardiacMonitor.IntegrationTests/
│       ├── ApiEndpointsTests.cs
│       └── CardiacMonitorApiFactory.cs
├── Program.cs
└── CardiacMonitor.slnx
```

The main project excludes the `tests` directory from its default SDK items. This prevents test source files and generated `bin/obj` artifacts from becoming part of the API output:

```xml
<DefaultItemExcludes>
  $(DefaultItemExcludes);tests\**
</DefaultItemExcludes>
```

## Centralized Exception Handling

`GlobalExceptionHandler` implements the .NET 8 `IExceptionHandler` interface. It catches exceptions that were not handled by an endpoint, writes the full exception to server logs, and returns a safe response to the client.

The handler is registered in `Program.cs`:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
```

The handler logs useful request context using structured message templates:

```csharp
_logger.LogError(
    exception,
    "Unhandled exception while processing {RequestMethod} {RequestPath}. TraceId: {TraceId}",
    httpContext.Request.Method,
    httpContext.Request.Path,
    httpContext.TraceIdentifier);
```

The placeholders are stored as separate log properties. This is more useful than interpolating one long text message because log aggregation systems can search by request method, path, or trace ID.

The client receives safe `ProblemDetails` instead of the exception message or stack trace:

```csharp
var problemDetails = new ProblemDetails
{
    Status = StatusCodes.Status500InternalServerError,
    Title = "An unexpected error occurred.",
    Detail = "The server could not process the request.",
    Instance = httpContext.Request.Path
};

problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
```

Example response:

```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "detail": "The server could not process the request.",
  "instance": "/api/patients",
  "traceId": "0HN..."
}
```

The response intentionally excludes:

- Exception messages.
- Stack traces.
- SQL statements.
- Connection strings.
- Internal class or file names.

The `traceId` lets developers match the safe client response to the detailed server log.

## Standardized ProblemDetails Responses

Expected controller errors now use `ProblemDetails` instead of unrelated anonymous response objects.

Previous style:

```csharp
return NotFound(new
{
    Message = $"Patient with ID {id} was not found."
});
```

Current style:

```csharp
return Problem(
    statusCode: StatusCodes.Status404NotFound,
    title: "Patient not found.",
    detail: $"Patient with ID {id} was not found.",
    instance: HttpContext.Request.Path);
```

This gives frontend clients a consistent shape for validation, authorization, not-found, rate-limit, and server errors.

### Error Statuses

| Status | Meaning | Response format |
| --- | --- | --- |
| `400 Bad Request` | Invalid request or failed business rule | `ProblemDetails` or `ValidationProblemDetails` |
| `401 Unauthorized` | Missing or invalid authentication | `ProblemDetails` |
| `403 Forbidden` | Valid identity without permission or ownership | `ProblemDetails` |
| `404 Not Found` | Requested entity does not exist | `ProblemDetails` |
| `429 Too Many Requests` | Rate limit exceeded | `ProblemDetails` |
| `500 Internal Server Error` | Unexpected server failure | Safe `ProblemDetails` with `traceId` |

## Authentication and Authorization Errors

Authorization middleware failures are processed by `ProblemDetailsAuthorizationMiddlewareResultHandler`.

The handler distinguishes between authentication and authorization:

```csharp
var statusCode = authorizeResult.Challenged
    ? StatusCodes.Status401Unauthorized
    : StatusCodes.Status403Forbidden;
```

- `401` means that the request does not contain a valid authenticated identity.
- `403` means that the identity is valid but does not have the required role or permission.

Ownership failures inside controllers also return explicit `403 ProblemDetails` responses:

```csharp
if (isPatient && patient.UserId != loggedInUserId)
{
    return Problem(
        statusCode: StatusCodes.Status403Forbidden,
        title: "Access forbidden.",
        detail: "Patients can access only their own profile.",
        instance: HttpContext.Request.Path);
}
```

## Rate-Limit Errors

The rate limiter now returns `application/problem+json` when a policy rejects a request:

```csharp
options.OnRejected = async (context, cancellationToken) =>
{
    var problemDetails = new ProblemDetails
    {
        Status = StatusCodes.Status429TooManyRequests,
        Title = "Too many requests.",
        Detail = "The request rate limit has been exceeded. Try again later.",
        Instance = context.HttpContext.Request.Path
    };

    await context.HttpContext.Response.WriteAsJsonAsync(
        problemDetails,
        options: null,
        contentType: "application/problem+json",
        cancellationToken: cancellationToken);
};
```

## Structured Authentication Logging

`AuthService` now receives `ILogger<AuthService>` through dependency injection.

A failed login caused by a missing user is logged without recording the submitted email address:

```csharp
_logger.LogWarning("Failed login attempt: user was not found.");
```

An invalid password is logged using the internal user ID rather than the password:

```csharp
_logger.LogWarning(
    "Failed login attempt for user {UserId}: invalid password.",
    user.Id);
```

Refresh-token validation logs only the exception type. Access tokens and refresh-token values are never written to logs:

```csharp
_logger.LogWarning(
    "Refresh token validation failed with {ExceptionType}.",
    exception.GetType().Name);
```

Expected token-validation failures return a controlled response. Unexpected failures are allowed to reach the global exception handler.

## Doctor-Role Validation

Previously, appointment creation checked only whether `DoctorId` existed in `AspNetUsers`. This allowed any identity user, including an administrator or patient, to be assigned as a doctor.

The service now checks the user's role through `AspNetUserRoles` and `AspNetRoles`:

```csharp
// Checks that the selected identity user belongs to the Doctor role.
private async Task<bool> IsDoctorAsync(string userId)
{
    return await (
        from userRole in _context.UserRoles
        join role in _context.Roles on userRole.RoleId equals role.Id
        where userRole.UserId == userId && role.NormalizedName == "DOCTOR"
        select userRole).AnyAsync();
}
```

Both appointment creation and update use the same check:

```csharp
if (!await IsDoctorAsync(request.DoctorId))
{
    return null;
}
```

The rule is covered by unit and integration tests:

- An ordinary identity user is rejected.
- An administrator is rejected as the selected doctor.
- A user assigned to the `Doctor` role is accepted.

## Unit Tests

The unit-test project uses:

- xUnit for the test framework.
- Moq for controlled service substitutes and interaction verification.
- EF Core InMemory for isolated appointment-service tests.

### Arrange-Act-Assert

Tests follow the Arrange-Act-Assert structure:

```csharp
// Arrange
var patientService = new Mock<IPatientService>();
patientService
    .Setup(service => service.GetPatientByIdAsync(999))
    .ReturnsAsync((PatientResponse?)null);

var controller = CreateController(
    patientService.Object,
    "admin-user",
    "Admin");

// Act
var result = await controller.GetById(999);

// Assert
var objectResult = Assert.IsType<ObjectResult>(result);
Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

patientService.Verify(
    service => service.GetPatientByIdAsync(999),
    Times.Once);
```

`Verify` confirms that the controller called its dependency exactly once with the expected identifier.

### Fact and Theory Tests

`Fact` covers one fixed scenario. `Theory` runs the same behavior against several inputs:

```csharp
[Theory]
[InlineData(29)]
[InlineData(251)]
[InlineData(500)]
public void Validate_ReturnsInvalidResult_WhenHeartRateIsOutsideRange(
    int heartRate)
{
    var validator = new CreateVitalSignRequestValidator();
    var request = new CreateVitalSignRequest(
        heartRate,
        98m,
        120,
        80);

    var result = validator.Validate(request);

    Assert.Contains(
        result.Errors,
        error => error.PropertyName ==
            nameof(CreateVitalSignRequest.HeartRate));
}
```

### Unit-Test Coverage

- Valid vital-sign input.
- Invalid low and high heart rates.
- Invalid oxygen saturation.
- Appointments in the past.
- Unsupported appointment statuses.
- Missing-patient controller response.
- Patient ownership success.
- Patient ownership rejection.
- Appointment rejection for a non-doctor user.
- Appointment acceptance for a Doctor-role user.

## Integration Tests

Integration tests start the complete API in memory using:

```csharp
WebApplicationFactory<Program>
```

The tests exercise:

- The real middleware pipeline.
- Routing and model binding.
- FluentValidation.
- JWT authentication.
- Role-based authorization.
- Ownership checks.
- Controller and service execution.
- Entity Framework Core.
- ProblemDetails serialization.

### Exposing Program to WebApplicationFactory

The following declaration was added after `app.Run()`:

```csharp
public partial class Program;
```

It gives the integration-test project access to the top-level application entry point without changing runtime behavior.

### Isolated SQLite Database

SQL Server is replaced only inside the test host:

```csharp
services.RemoveAll<DbContextOptions<AppDbContext>>();

services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(_connection));
```

The SQLite connection uses `Data Source=:memory:` and stays open for the lifetime of the test factory. This provides:

- A real relational database engine.
- Foreign-key and relationship behavior.
- Fast execution.
- No changes to the development database.
- Repeatable test data.

The test host also uses ephemeral data-protection keys and console logging so it does not depend on a Windows user profile or Event Log permissions.

### Signed Test JWTs

Authenticated endpoints are tested using a real signed JWT:

```csharp
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, userId),
    new Claim(ClaimTypes.NameIdentifier, userId),
    new Claim(ClaimTypes.Role, role)
};

var token = new JwtSecurityToken(
    issuer: JwtIssuer,
    audience: JwtAudience,
    expires: DateTime.UtcNow.AddMinutes(10),
    claims: claims,
    signingCredentials: new SigningCredentials(
        signingKey,
        SecurityAlgorithms.HmacSha256));
```

This verifies the real JWT bearer configuration rather than bypassing authentication with a fake handler.

### Integration-Test Coverage

- Missing JWT returns `401 ProblemDetails`.
- Valid Admin JWT can retrieve patients.
- Missing patient returns `404 ProblemDetails`.
- Patient ownership violation returns `403 ProblemDetails`.
- Invalid vital signs return `400 ValidationProblemDetails`.
- A non-doctor user cannot be assigned as an appointment doctor.
- An unexpected service exception returns safe `500 ProblemDetails`.
- Sensitive exception text is not included in the HTTP response.

## Running the Tests

Restore all packages:

```powershell
dotnet restore CardiacMonitor.slnx
```

Build the complete solution:

```powershell
dotnet build CardiacMonitor.slnx --no-restore
```

Run every test:

```powershell
dotnet test CardiacMonitor.slnx --no-build --no-restore
```

Run only unit tests:

```powershell
dotnet test tests/CardiacMonitor.UnitTests/CardiacMonitor.UnitTests.csproj
```

Run only integration tests:

```powershell
dotnet test tests/CardiacMonitor.IntegrationTests/CardiacMonitor.IntegrationTests.csproj
```

## Current Test Result

```text
CardiacMonitor.UnitTests
Passed: 15
Failed: 0

CardiacMonitor.IntegrationTests
Passed: 7
Failed: 0

Total
Passed: 22
Failed: 0
```

## Method Comments

Every newly introduced method has a short English `//` comment immediately before it. The comments describe responsibility rather than restating the method name. Example:

```csharp
// Checks that the selected identity user belongs to the Doctor role.
private async Task<bool> IsDoctorAsync(string userId)
```

```csharp
// Creates an isolated in-memory database context for each unit test.
private static AppDbContext CreateContext()
```

```csharp
// Generates a valid test JWT containing identity and role claims.
private static string CreateJwt(string userId, string role)
```

 