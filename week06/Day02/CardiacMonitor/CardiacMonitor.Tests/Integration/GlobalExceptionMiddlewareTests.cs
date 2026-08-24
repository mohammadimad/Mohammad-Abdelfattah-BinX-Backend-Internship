using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CardiacMonitor.Tests.Integration;

public sealed class GlobalExceptionMiddlewareTests
    : IClassFixture<CardiacMonitorWebApplicationFactory>
{
    private readonly HttpClient _client;

      // HttpClient is created in memory and sends the request through the full application path.
     public GlobalExceptionMiddlewareTests(CardiacMonitorWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

      //Ensures that the unhandled exception is converted into a safe and uniform ProblemDetails response.
     [Fact]
    public async Task UnhandledException_ReturnsSafeProblemDetails()
    {
        // Act 
        var response = await _client.GetAsync("/api/diagnostics/unhandled-error");
        var responseBody = await response.Content.ReadAsStringAsync();
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert 
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("An unexpected error occurred.", problem.Title);
        Assert.Equal("/api/diagnostics/unhandled-error", problem.Instance);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
        Assert.DoesNotContain("Diagnostic exception details", responseBody);
        Assert.DoesNotContain(nameof(InvalidOperationException), responseBody);
    }
}
