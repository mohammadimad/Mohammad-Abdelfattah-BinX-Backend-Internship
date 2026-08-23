using Microsoft.AspNetCore.Mvc;

namespace CardiacMonitor.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

     /// Stores the next pipeline step and the logger used for server-side errors.
     public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

     /// Runs the remaining pipeline and converts unhandled exceptions to safe ProblemDetails.
     public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}. Trace ID: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteProblemDetailsAsync(context);
        }
    }

     /// Writes a generic HTTP 500 response without exposing exception details.
     private static async Task WriteProblemDetailsAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "Please contact support with the provided trace ID if the problem persists.",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
