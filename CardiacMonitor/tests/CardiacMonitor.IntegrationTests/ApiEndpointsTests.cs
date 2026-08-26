using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace CardiacMonitor.IntegrationTests;

public class ApiEndpointsTests : IClassFixture<CardiacMonitorApiFactory>
{
    private const string JwtKey =
        "SuperSecretKeyThatIsAtLeast32CharactersLongAndVerySecure!";
    private const string JwtIssuer = "CardiacMonitorAPI";
    private const string JwtAudience = "CardiacMonitorAPI";
    private readonly CardiacMonitorApiFactory _factory;

    // Stores the shared API factory used by each integration test.
    public ApiEndpointsTests(CardiacMonitorApiFactory factory)
    {
        _factory = factory;
    }

    // Verifies that a protected endpoint returns ProblemDetails without a token.
    [Fact]
    public async Task GetPatients_ReturnsUnauthorizedProblemDetails_WhenTokenIsMissing()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var response = await client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Authentication required.", problem?.Title);
    }

    // Verifies that an authenticated administrator can read the patient list.
    [Fact]
    public async Task GetPatients_ReturnsOk_WhenAdminTokenIsValid()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("admin-user", "Admin");

        // Act
        var response = await client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var patients = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.NotNull(patients);
        Assert.NotEmpty(patients);
    }

    // Verifies that a missing patient returns a standardized not-found response.
    [Fact]
    public async Task GetPatient_ReturnsNotFoundProblemDetails_WhenPatientDoesNotExist()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("admin-user", "Admin");

        // Act
        var response = await client.GetAsync("/api/patients/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Patient not found.", problem?.Title);
    }

    // Verifies that ownership checks forbid access to another patient's profile.
    [Fact]
    public async Task GetPatient_ReturnsForbiddenProblemDetails_WhenPatientDoesNotOwnProfile()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("patient-user", "Patient");

        // Act
        var response = await client.GetAsync("/api/patients/2");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Access forbidden.", problem?.Title);
    }

    // Verifies that invalid vital signs produce ValidationProblemDetails.
    [Fact]
    public async Task CreateVitalSign_ReturnsValidationProblem_WhenValuesAreInvalid()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("admin-user", "Admin");
        var request = new CreateVitalSignRequest(500, 150m, 300, 10);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/patients/1/vitals",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(nameof(CreateVitalSignRequest.HeartRate), problem.Errors.Keys);
    }

    // Verifies that an administrator cannot be assigned as an appointment doctor.
    [Fact]
    public async Task CreateAppointment_ReturnsBadRequest_WhenSelectedUserIsNotDoctor()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("admin-user", "Admin");
        var request = new CreateAppointmentRequest(
            "admin-user",
            DateTime.UtcNow.AddDays(1),
            "Scheduled",
            null);

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/patients/1/appointments",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Contains("Doctor role", problem?.Detail);
    }

    // Verifies that unhandled exceptions are hidden behind safe ProblemDetails.
    [Fact]
    public async Task GetPatients_ReturnsSafeProblemDetails_WhenServiceThrows()
    {
        // Arrange
        const string sensitiveMessage = "Sensitive database details";
        var patientService = new Mock<IPatientService>();
        patientService
            .Setup(service => service.GetAllPatientsAsync())
            .ThrowsAsync(new InvalidOperationException(sensitiveMessage));

        using var throwingFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IPatientService>();
                services.AddSingleton(patientService.Object);
            });
        });
        using var client = CreateAuthenticatedClient(
            throwingFactory,
            "admin-user",
            "Admin");

        // Act
        var response = await client.GetAsync("/api/patients");
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain(sensitiveMessage, responseBody);
        Assert.Contains("traceId", responseBody);
    }

    // Creates an HTTPS test client without following redirects.
    private HttpClient CreateClient()
    {
        return CreateClient(_factory);
    }

    // Creates an HTTPS test client from the selected application factory.
    private static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    // Creates a test client with a signed JWT for the selected user and role.
    private HttpClient CreateAuthenticatedClient(string userId, string role)
    {
        return CreateAuthenticatedClient(_factory, userId, role);
    }

    // Creates an authenticated client from a customized application factory.
    private static HttpClient CreateAuthenticatedClient(
        WebApplicationFactory<Program> factory,
        string userId,
        string role)
    {
        var client = CreateClient(factory);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateJwt(userId, role));
        return client;
    }

    // Generates a valid test JWT containing identity and role claims.
    private static string CreateJwt(string userId, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, $"{userId}@test.local"),
            new Claim(ClaimTypes.Role, role)
        };
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            expires: DateTime.UtcNow.AddMinutes(10),
            claims: claims,
            signingCredentials: new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
