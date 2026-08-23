# Week 5 - Day 4: Centralized Error Handling

## Day Overview

Day 4 focused on replacing repeated endpoint-level `try/catch` blocks with one global exception-handling middleware. Unhandled exceptions are logged on the server and returned to clients as safe, standardized `ProblemDetails` responses.

## What We Learned

- Why scattered `try/catch` blocks create duplication and inconsistent responses.
- How middleware can catch exceptions from downstream endpoints.
- How to return the `ProblemDetails` error format.
- Why exception messages and stack traces must not be exposed to clients.
- How structured logging preserves the HTTP method, path, and trace ID as searchable values.
- How to test middleware through the complete HTTP pipeline.

## Tasks We Completed

### Task 1: Register the global middleware

The middleware is placed early in the pipeline so it can catch exceptions from the components that follow it.

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
```

### Task 2: Log unhandled exceptions with context

The complete exception is recorded server-side with structured request information.

```csharp
_logger.LogError(
    exception,
    "Unhandled exception while processing {Method} {Path}. Trace ID: {TraceId}",
    context.Request.Method,
    context.Request.Path,
    context.TraceIdentifier);
```

### Task 3: Return a safe ProblemDetails response

The client receives a generic message and trace ID without receiving internal exception details.

```csharp
var problemDetails = new ProblemDetails
{
    Status = StatusCodes.Status500InternalServerError,
    Title = "An unexpected error occurred.",
    Instance = context.Request.Path
};

problemDetails.Extensions["traceId"] = context.TraceIdentifier;
```

### Task 4: Verify that sensitive details are not leaked

An integration test deliberately triggers an exception and inspects the response.

```csharp
Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
Assert.DoesNotContain("Diagnostic exception details", responseBody);
Assert.DoesNotContain(nameof(InvalidOperationException), responseBody);
```

## Files Related to Day 4

- `Middlewares/GlobalExceptionMiddleware.cs`
- `Controllers/DiagnosticsController.cs`
- `CardiacMonitor.Tests/Integration/GlobalExceptionMiddlewareTests.cs`
- `Program.cs`

## Day Result

Unhandled failures now produce consistent HTTP 500 ProblemDetails responses, full details are logged on the server, and internal exception information is not exposed to API clients.
