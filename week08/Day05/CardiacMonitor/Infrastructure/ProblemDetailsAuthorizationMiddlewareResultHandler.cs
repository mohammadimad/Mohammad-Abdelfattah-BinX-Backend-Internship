using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

namespace CardiacMonitor.Infrastructure;

public sealed class ProblemDetailsAuthorizationMiddlewareResultHandler
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    // Returns ProblemDetails for authorization failures and delegates successful requests.
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged || authorizeResult.Forbidden)
        {
            var statusCode = authorizeResult.Challenged
                ? StatusCodes.Status401Unauthorized
                : StatusCodes.Status403Forbidden;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = authorizeResult.Challenged
                    ? "Authentication required."
                    : "Access forbidden.",
                Detail = authorizeResult.Challenged
                    ? "A valid bearer token is required to access this resource."
                    : "You do not have permission to access this resource.",
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            if (authorizeResult.Challenged)
            {
                context.Response.Headers.WWWAuthenticate = "Bearer";
            }

            await context.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: context.RequestAborted);
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
