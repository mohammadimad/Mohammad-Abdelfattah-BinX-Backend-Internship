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
using Xunit;

namespace CardiacMonitor.Tests.Integration;

// WebApplicationFactory The application runs entirely within memory and gives us HttpClient without opening a real network port..
public sealed class CardiacMonitorWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"CardiacMonitorTests-{Guid.NewGuid()}";

    public CardiacMonitorWebApplicationFactory()
    {
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // We replace SQL Server with a separate InMemory database so that tests do not change development data.
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        //Recreating the rule makes each test independent and repeatable in any order.
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}

public class PatientsApiTests : IClassFixture<CardiacMonitorWebApplicationFactory>
{
    private readonly CardiacMonitorWebApplicationFactory _factory;

    public PatientsApiTests(CardiacMonitorWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    //We get a real JWT from endpoint login to test endpoint protected.
    private static async Task<string> GetDoctorJwtTokenAsync(HttpClient client)
    {
        var loginRequest = new LoginRequest("doctor@cardiac.com", "Doctor@123");

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResult!.Token!;
    }

    [Fact]
    public async Task GetPatientById_WithValidToken_ReturnsOkAndPatient()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetDoctorJwtTokenAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var patientId = 1;

        // Act: A genuine HTTP request goes through routing, middleware, and DI.
        var response = await client.GetAsync($"/api/patients/{patientId}");

        // Assert:We examine the case and all the required response data in the Day03 laboratory.
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

        // Assert: This is the error path for the same endpoint.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPatientById_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var patientId = 1;

        // Act: We do not intentionally add the Authorization header.
        var response = await client.GetAsync($"/api/patients/{patientId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
