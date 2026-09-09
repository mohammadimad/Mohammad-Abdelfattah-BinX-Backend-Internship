# Week 7 - Day 4: Custom Middleware & Cross-Cutting Concerns

## Day Overview

Day 4 focused on identifying and addressing a genuine cross-cutting concern across our capstone project [8]. We implemented a custom `RequestTimingMiddleware` using `Stopwatch` and `ILogger` to dynamically measure and log HTTP request/response latency globally, and prepared our Sprint 2 codebase for a structured pull request and mentor code review [8].

## What We Learned

- Identifying genuine cross-cutting concerns (such as request logging, tracing, and global performance monitoring) that apply broadly across all endpoints [7].
- Why duplicating timing or diagnostic logging inside individual controllers is highly repetitive and error-prone [7].
- The architectural difference between Middleware (operates globally on every HTTP request/response) and Action Filters (operates closer to specific controller actions with model-binding access) [8].
- Structuring focused commits and writing descriptive pull requests to facilitate effective code reviews [8].

## Tasks We Completed

### Task 1: Identify a Genuine Cross-Cutting Concern

We identified **Request Latency Diagnostics** as a critical cross-cutting concern for our Cardiac Patient Monitoring System [8]. Since our API processes continuous patient telemetry data (vital signs), tracing and logging how many milliseconds each HTTP request takes is vital for diagnosing database bottlenecks without writing repetitive timing boilerplate code inside each controller [7, 8].

---

### Task 2: Implement and Register Custom Middleware

We built a custom `RequestTimingMiddleware` utilizing `Stopwatch` to track pipeline duration and logged the output using structured parameterized logging via `ILogger` [8].

```csharp
// Inside Middlewares/RequestTimingMiddleware.cs
using System.Diagnostics;

namespace CardiacMonitor.Middlewares;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start the stopwatch before passing the request downstream
        var sw = Stopwatch.StartNew();

        await _next(context); // Pass the HTTP context to the next middleware

        sw.Stop();

        // Log request method, path, response status, and duration (Structured Logging)
        _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {Elapsed}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}
```

#### Middleware Pipeline Registration

We registered our custom middleware in `Program.cs` right after `UseHttpsRedirection()` to ensure it measures the complete execution time of all authentication, authorization, and controller middlewares [8].

```csharp
// Inside Program.cs
app.UseHttpsRedirection();

// Register the custom timing middleware for Day 4
app.UseMiddleware<RequestTimingMiddleware>();

app.UseCors("AllowFrontendOnly");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
```

### Task 3: Verify Consistent Pipeline Execution

We ran the API and triggered multiple endpoints (such as `/api/auth/login` and `/api/patients/1`). Without making any code changes to individual controllers, our console successfully printed consistent timing metrics for every single incoming request:

```text
info: CardiacMonitor.Middlewares.RequestTimingMiddleware[0]
      HTTP POST /api/auth/login responded 200 in 45ms
info: CardiacMonitor.Middlewares.RequestTimingMiddleware[0]
      HTTP GET /api/patients/1 responded 200 in 12ms
```

---

### Task 4 & 5: Prepare and Open the Sprint 2 Pull Request (PR)

We prepared our codebase for a structured code review by committing our finalized changes and pushing them to our remote repository [8].

- **Branch Clean-up:** Checked out our feature branch `git checkout -b feature/sprint2-auth-rbac-middleware` [5, 8].
- **DESCRIPTIVE Commits:** Staged and committed changes with clear, imperative messages [8]:
  - **feat: Add custom RequestTimingMiddleware and integrate in Program.cs** [8]
  - **feat: Implement unified Register transaction and custom PatientId claim** [5]
  - **feat: Secure clinical controllers with explicit RBAC and ownership checks** [6, 7]
- **Open Pull Request:** Pushed the branch to GitHub and opened a clean, descriptive pull request summarizing our Authentication, RBAC, and Middleware accomplishments, ready for mentor review [8].

## Files Related to Day 4

- `Middlewares/RequestTimingMiddleware.cs` (Measures request duration) [8]
- `Program.cs` (Pipeline registration) [8]

## Day Result

Our cross-cutting concern is cleanly solved globally [8]. The custom timing middleware successfully measures and logs request performance across all endpoints automatically, and our completed Sprint 2 branch has been pushed and opened as a reviewed pull request on GitHub, marking all development work for Sprint 2 complete [8]!
