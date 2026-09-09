using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CardiacMonitor.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

     //The next step is saved in the request path and the logger used to record the error on the server.
     public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

      //It runs the rest of the request path and converts any unhandled exception to a safe ProblemDetails.
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

            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "The error response could not be written because the response had already started. Trace ID: {TraceId}",
                    context.TraceIdentifier);
                throw;
            }

            await WriteProblemDetailsAsync(context);
        }
    }

    //It writes a generic HTTP 500 response without disclosing the exception message or its details to the client.
    private static async Task WriteProblemDetailsAsync(HttpContext context)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "Please contact support with the provided trace ID if the problem persists.",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problemDetails,
            JsonOptions,
            context.RequestAborted);
    }
}
