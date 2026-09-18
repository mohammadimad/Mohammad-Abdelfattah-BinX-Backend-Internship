using System.Diagnostics;

namespace CardiacMonitor.Infrastructure;

public sealed class DatabasePerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DatabasePerformanceMiddleware> _logger;

    // Stores the next pipeline component and performance logger.
    public DatabasePerformanceMiddleware(
        RequestDelegate next,
        ILogger<DatabasePerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Logs request duration and executed SQL command count in development.
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var queryCount = context.Items.TryGetValue(
            DatabaseQueryCounterInterceptor.QueryCountItemKey,
            out var value) && value is int count
                ? count
                : 0;

        _logger.LogInformation(
            "Database profile {Method} {Path}: {QueryCount} queries in {ElapsedMilliseconds} ms (HTTP {StatusCode}).",
            context.Request.Method,
            context.Request.Path,
            queryCount,
            stopwatch.Elapsed.TotalMilliseconds,
            context.Response.StatusCode);
    }
}
