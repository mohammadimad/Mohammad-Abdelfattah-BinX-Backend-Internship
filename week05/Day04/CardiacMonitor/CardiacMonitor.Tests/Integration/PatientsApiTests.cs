using CardiacMonitor;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CardiacMonitor.Tests.Integration;

// Hosts the complete API in memory and provides an HttpClient without a real network port.
public sealed class CardiacMonitorWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"CardiacMonitorTests-{Guid.NewGuid()}";

    // Sets an HTTPS base address for integration-test clients.
    public CardiacMonitorWebApplicationFactory()
    {
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    // Replaces production-only dependencies with isolated test dependencies.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Keep ILogger active without depending on the Windows Event Log.
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddDebug();
        });

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server with the isolated InMemory provider described in Day 3.
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }

    // Recreates the test database so each test starts from the same data.
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}

public class PatientsApiTests : IClassFixture<CardiacMonitorWebApplicationFactory>
{
    private readonly CardiacMonitorWebApplicationFactory _factory;

    // Stores the shared factory and resets its isolated database.
    public PatientsApiTests(CardiacMonitorWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    // Gets a real JWT from the login endpoint for a protected-route test.
    private static async Task<string> GetDoctorJwtTokenAsync(HttpClient client)
    {
        var loginRequest = new LoginRequest("doctor@cardiac.com", "Doctor@123");

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResult!.Token!;
    }

    // Verifies the happy path and the complete patient response body.
    [Fact]
    public async Task GetPatientById_WithValidToken_ReturnsOkAndPatient()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetDoctorJwtTokenAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var patientId = 1;

        // Act: Send a real HTTP request through routing, middleware, and DI.
        var response = await client.GetAsync($"/api/patients/{patientId}");

        // Assert: Check the status and the complete response body.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var patient = await response.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(patient);
        Assert.Equal(patientId, patient.Id);
        Assert.Null(patient.UserId);
        Assert.Equal("Ahmad", patient.FirstName);
        Assert.Equal("Amr", patient.LastName);
        Assert.Equal(new DateTime(1990, 5, 12), patient.DateOfBirth);
        Assert.Equal("Male", patient.Gender);
        Assert.Equal("+9759835279", patient.ContactNumber);
    }

    // Verifies the not-found path for the same patient endpoint.
    [Fact]
    public async Task GetPatientById_WhenPatientDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetDoctorJwtTokenAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var nonExistentPatientId = 99999;

        // Act
        var response = await client.GetAsync($"/api/patients/{nonExistentPatientId}");

        // Assert: Check the not-found path for the same endpoint.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Verifies that the protected endpoint rejects a request without a JWT.
    [Fact]
    public async Task GetPatientById_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var patientId = 1;

        // Act: Send the request without an Authorization header.
        var response = await client.GetAsync($"/api/patients/{patientId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
