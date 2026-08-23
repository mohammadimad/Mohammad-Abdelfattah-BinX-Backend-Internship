using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CardiacMonitor.Tests.Integration;

public sealed class GlobalExceptionMiddlewareTests
    : IClassFixture<CardiacMonitorWebApplicationFactory>
{
    private readonly HttpClient _client;

     /// Creates an in-memory client that sends requests through the complete API pipeline.
     public GlobalExceptionMiddlewareTests(CardiacMonitorWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

     /// Verifies that an unhandled exception becomes a safe ProblemDetails response.
     [Fact]
    public async Task UnhandledException_ReturnsSafeProblemDetails()
    {
        // Act: This endpoint deliberately throws an unhandled exception.
        var response = await _client.GetAsync("/api/diagnostics/unhandled-error");
        var responseBody = await response.Content.ReadAsStringAsync();
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert: Check the standard shape and confirm that internal details are hidden.
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("An unexpected error occurred.", problem.Title);
        Assert.Equal("/api/diagnostics/unhandled-error", problem.Instance);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
        Assert.DoesNotContain("Diagnostic exception details", responseBody);
        Assert.DoesNotContain(nameof(InvalidOperationException), responseBody);
    }
}
