# Week 5 - Day 3: Integration Testing with WebApplicationFactory

## Day Overview

Day 3 focused on testing the API through its real HTTP pipeline. `WebApplicationFactory<Program>` starts the application in memory and provides an `HttpClient` that exercises routing, middleware, authentication, dependency injection, serialization, and controllers together.

## What We Learned

- The difference between unit tests and integration tests.
- How `WebApplicationFactory` hosts an ASP.NET Core API in memory.
- How to send real HTTP requests without opening a network port.
- How to replace SQL Server with an isolated EF Core InMemory database.
- How to reset test data so tests remain repeatable.
- How to attach a valid JWT to an authenticated request.

## Tasks We Completed

### Task 1: Create a WebApplicationFactory

The custom factory starts the API in the `Testing` environment.

```csharp
public sealed class CardiacMonitorWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
```

### Task 2: Use an isolated test database

The production SQL Server registration is replaced with a uniquely named InMemory database.

```csharp
services.RemoveAll<DbContextOptions<AppDbContext>>();

services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(_databaseName);
});
```

### Task 3: Test the authenticated happy path

The test logs in through the real authentication endpoint, attaches the returned JWT, and verifies the full patient response.

```csharp
var token = await GetDoctorJwtTokenAsync(client);
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

var response = await client.GetAsync($"/api/patients/{patientId}");

Assert.Equal(HttpStatusCode.OK, response.StatusCode);
```

### Task 4: Test error paths

The same endpoint is tested for a missing patient and for a request without a JWT.

```csharp
Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
```

```csharp
Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
```

## Files Related to Day 3

- `CardiacMonitor.Tests/Integration/PatientsApiTests.cs`
- `CardiacMonitor.Tests/CardiacMonitor.Tests.csproj`
- `Program.cs`

## Day Result

The primary patient endpoint is covered through the complete HTTP pipeline with happy, not-found, and unauthorized scenarios, including a valid JWT and an isolated test database.
