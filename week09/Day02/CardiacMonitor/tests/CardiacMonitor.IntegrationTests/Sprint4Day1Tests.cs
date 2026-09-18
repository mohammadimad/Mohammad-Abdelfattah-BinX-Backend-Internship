using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CardiacMonitor.IntegrationTests;

public class Sprint4Day1Tests
{
    // Gap 1: exercise real Identity password checking without leaking account existence.
    [Theory]
    [InlineData("unknown@test.local", "ValidPassword1!")]
    [InlineData("day1@test.local", "WrongPassword1!")]
    public async Task Login_RejectsInvalidCredentials(string email, string password)
    {
        using var factory = new CardiacMonitorApiFactory();
        using var client = CreateClient(factory);
        await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Invalid email or password.", problem?.Detail);
    }

    [Fact]
    public async Task Register_RejectsDuplicateEmail_WithoutCreatingAnotherProfile()
    {
        using var factory = new CardiacMonitorApiFactory();
        using var client = CreateClient(factory);
        await RegisterAsync(client);

        var response = await client.PostAsJsonAsync("/api/auth/register", Registration());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == "day1@test.local");
        Assert.Equal(1, await db.Patients.CountAsync(p => p.UserId == user.Id));
    }

    // Gap 2: rotate a genuine persisted token pair, then reject replay of that pair.
    [Fact]
    public async Task Refresh_RotatesTokenPair_AndRejectsReplay()
    {
        using var factory = new CardiacMonitorApiFactory();
        using var client = CreateClient(factory);
        await RegisterAsync(client);
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("day1@test.local", "ValidPassword1!"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var original = (await login.Content.ReadFromJsonAsync<AuthResponse>())!;

        // Preserve the real JTI and identity; re-sign with a past expiry to avoid waiting.
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(original.Token);
        var claims = jwt.Claims.Where(c => c.Type is not "exp" and not "nbf" and not "iat");
        var expiredAccess = SignToken(factory, claims, DateTime.UtcNow.AddMinutes(-10));
        var request = new TokenRequest(expiredAccess, original.RefreshToken!);
        var refreshed = await client.PostAsJsonAsync("/api/auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
        var replacement = (await refreshed.Content.ReadFromJsonAsync<AuthResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(replacement.Token));
        Assert.NotEqual(original.RefreshToken, replacement.RefreshToken);
        var replay = await client.PostAsJsonAsync("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        var problem = await replay.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Refresh token has already been used.", problem?.Detail);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True((await db.RefreshTokens.SingleAsync(
            t => t.Token == original.RefreshToken)).IsUsed);
        Assert.False((await db.RefreshTokens.SingleAsync(
            t => t.Token == replacement.RefreshToken)).IsUsed);
    }

    [Fact]
    public async Task Refresh_RejectsMalformedAccessToken()
    {
        using var factory = new CardiacMonitorApiFactory();
        using var client = CreateClient(factory);
        var response = await client.PostAsJsonAsync("/api/auth/refresh",
            new TokenRequest("not-a-jwt", "not-a-refresh-token"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Token refresh failed.", problem?.Title);
    }

    // Gap 3: each read must allow the owner and independently deny another patient.
    [Theory]
    [InlineData("")]
    [InlineData("/clinical-details")]
    [InlineData("/vitals")]
    [InlineData("/medications")]
    [InlineData("/appointments")]
    [InlineData("/alerts")]
    public async Task PatientReads_EnforceResourceOwnership(string suffix)
    {
        using var factory = new CardiacMonitorApiFactory();
        using var client = CreateAuthenticatedClient(factory, "patient-user", "Patient");

        var own = await client.GetAsync($"/api/patients/1{suffix}");
        var other = await client.GetAsync($"/api/patients/2{suffix}");

        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, other.StatusCode);
        Assert.Equal("application/problem+json", other.Content.Headers.ContentType?.MediaType);
        if (suffix == "/clinical-details")
        {
            var detail = await own.Content.ReadFromJsonAsync<PatientClinicalDetailsResponse>();
            Assert.Equal(1, detail?.Patient.Id);
            Assert.NotEmpty(detail!.VitalSigns);
            Assert.All(detail.VitalSigns, vital => Assert.Equal(1, vital.PatientId));
        }
    }

    // Gap 4: exercise real authorization middleware, not a direct controller call.
    [Theory]
    [InlineData("POST", "/api/staff/doctors")]
    [InlineData("POST", "/api/staff/nurses")]
    [InlineData("POST", "/api/patients")]
    [InlineData("POST", "/api/patients/1/appointments")]
    [InlineData("POST", "/api/patients/1/medications")]
    [InlineData("POST", "/api/patients/1/care-assignments")]
    [InlineData("POST", "/api/doctors/1/availability")]
    [InlineData("PUT", "/api/patients/1")]
    [InlineData("PUT", "/api/vitals/1")]
    [InlineData("PUT", "/api/medications/1")]
    [InlineData("PUT", "/api/appointments/1")]
    [InlineData("DELETE", "/api/patients/1")]
    [InlineData("DELETE", "/api/vitals/1")]
    [InlineData("DELETE", "/api/medications/1")]
    [InlineData("DELETE", "/api/appointments/1")]
    [InlineData("DELETE", "/api/care-assignments/1")]
    [InlineData("DELETE", "/api/doctor-availability/1")]
    [InlineData("PATCH", "/api/alerts/1/acknowledge")]
    [InlineData("PATCH", "/api/alerts/1/resolve")]
    public async Task PrivilegedWrites_RejectPatientRole(string method, string route)
    {
        using var factory = new CardiacMonitorApiFactory();
        using var client = CreateAuthenticatedClient(factory, "patient-user", "Patient");
        using var request = new HttpRequestMessage(new HttpMethod(method), route)
        {
            Content = JsonContent.Create(new { })
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(403, problem?.Status);
        Assert.Equal("0", response.Headers.GetValues(QueryCountStartupFilter.HeaderName).Single());
    }

    // Gap 5 / Sprint 3 action: COUNT + page remain two SQL commands as result size grows.
    [Fact]
    public async Task PatientCatalog_QueryCountRemainsTwo_ForSmallAndLargePages()
    {
        using var factory = new CardiacMonitorApiFactory();
        using var client = CreateAuthenticatedClient(factory, "admin-user", "Admin");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Patients.AddRange(Enumerable.Range(1, 60).Select(i => new Patient
            {
                FirstName = $"Regression{i:D2}", LastName = "Patient", Gender = "Male",
                DateOfBirth = new DateTime(1990, 1, 1), ContactNumber = "+970599111111"
            }));
            await db.SaveChangesAsync();
        }

        foreach (var size in new[] { 1, 50 })
        {
            var response = await client.GetAsync($"/api/patients?page=1&pageSize={size}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<PatientResponse>>();
            Assert.Equal(size, result?.Items.Count);
            Assert.Equal(62, result?.TotalCount);
            Assert.Equal("2", response.Headers.GetValues(QueryCountStartupFilter.HeaderName).Single());
        }
    }

    private static RegisterRequest Registration() => new(
        "day1@test.local", "ValidPassword1!", "DayOne", "Patient",
        new DateTime(1995, 1, 1), "Male", "+970599111111");

    private static async Task RegisterAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", Registration());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static HttpClient CreateClient(CardiacMonitorApiFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
        });

    private static HttpClient CreateAuthenticatedClient(
        CardiacMonitorApiFactory factory, string userId, string role)
    {
        var client = CreateClient(factory);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", SignToken(factory, claims, DateTime.UtcNow.AddMinutes(10)));
        return client;
    }

    private static string SignToken(
        CardiacMonitorApiFactory factory, IEnumerable<Claim> claims, DateTime expires)
    {
        var config = factory.Services.GetRequiredService<IConfiguration>();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: config["Jwt:Issuer"], audience: config["Jwt:Audience"],
            claims: claims, expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)));
    }
}
