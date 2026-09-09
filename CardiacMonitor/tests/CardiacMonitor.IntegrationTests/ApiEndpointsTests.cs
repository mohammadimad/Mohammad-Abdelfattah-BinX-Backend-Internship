using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Infrastructure;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
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
        var page = await response.Content
            .ReadFromJsonAsync<PagedResult<PatientResponse>>();
        Assert.NotNull(page);
        Assert.NotEmpty(page.Items);
    }

    // Verifies that the patient list supports search, gender filtering, sorting, and pagination.
    [Fact]
    public async Task GetPatients_ReturnsRequestedFilteredPage()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("admin-user", "Admin");

        // Act
        var response = await client.GetAsync(
            "/api/patients?page=1&pageSize=1&search=Sara&gender=female&sort=lastName_asc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content
            .ReadFromJsonAsync<PagedResult<PatientResponse>>();
        Assert.NotNull(page);
        Assert.Single(page.Items);
        Assert.Equal("Sara", page.Items[0].FirstName);
        Assert.Equal("Female", page.Items[0].Gender);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(1, page.TotalPages);
    }

    // Verifies that an unsafe patient page size returns ValidationProblemDetails.
    [Fact]
    public async Task GetPatients_ReturnsValidationProblem_WhenPageSizeIsTooLarge()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("admin-user", "Admin");

        // Act
        var response = await client.GetAsync("/api/patients?pageSize=101");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(nameof(PatientQueryParameters.PageSize), problem.Errors.Keys);
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

    // Verifies that vital-sign history supports filtering, sorting, and pagination.
    [Fact]
    public async Task GetVitalSigns_ReturnsRequestedFilteredPage()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("admin-user", "Admin");

        // Act
        var response = await client.GetAsync(
            "/api/patients/1/vitals?page=1&pageSize=1&minHeartRate=80&sort=heartRate_desc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content
            .ReadFromJsonAsync<PagedResult<VitalSignResponse>>();
        Assert.NotNull(page);
        Assert.Single(page.Items);
        Assert.Equal(82, page.Items[0].HeartRate);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(1, page.TotalPages);
    }

    // Verifies that an unsafe page size returns ValidationProblemDetails.
    [Fact]
    public async Task GetVitalSigns_ReturnsValidationProblem_WhenPageSizeIsTooLarge()
    {
        // Arrange
        using var client = CreateAuthenticatedClient("admin-user", "Admin");

        // Act
        var response = await client.GetAsync(
            "/api/patients/1/vitals?pageSize=101");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(nameof(VitalSignQueryParameters.PageSize), problem.Errors.Keys);
    }

    // Verifies that public registration cannot self-assign a privileged role.
    [Fact]
    public async Task Register_AssignsPatientRole_WhenBodyContainsInjectedRole()
    {
        // Arrange
        var email = $"role-injection-{Guid.NewGuid():N}@test.local";
        using var client = CreateClient();
        var request = new
        {
            Email = email,
            Password = "ValidPassword1!",
            FirstName = "Secure",
            LastName = "Patient",
            DateOfBirth = new DateTime(1995, 1, 1),
            Gender = "Male",
            ContactNumber = "+970599111111",
            Role = "Admin"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await context.Users.SingleAsync(entity => entity.Email == email);
        var roles = await (
            from userRole in context.UserRoles
            join role in context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == user.Id
            select role.NormalizedName).ToListAsync();
        Assert.Contains("PATIENT", roles);
        Assert.DoesNotContain("ADMIN", roles);
    }

    // Verifies that registration commits identity, Patient role, and domain profile.
    [Fact]
    public async Task Register_CreatesIdentityRoleAndPatientProfile_WhenRequestIsValid()
    {
        // Arrange
        var email = $"registered-{Guid.NewGuid():N}@test.local";
        using var client = CreateClient();
        var request = new RegisterRequest(
            email,
            "ValidPassword1!",
            "Registered",
            "Patient",
            new DateTime(1992, 6, 15),
            "Female",
            "+970599222222");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await context.Users.SingleAsync(entity => entity.Email == email);
        var patientRole = await context.Roles
            .SingleAsync(role => role.NormalizedName == "PATIENT");
        Assert.True(await context.UserRoles.AnyAsync(userRole =>
            userRole.UserId == user.Id && userRole.RoleId == patientRole.Id));
        var patient = await context.Patients.SingleAsync(entity =>
            entity.UserId == user.Id);
        Assert.Equal("Registered", patient.FirstName);
        Assert.Equal("Female", patient.Gender);
    }

    // Verifies that login includes the linked Patient domain identifier in the JWT.
    [Fact]
    public async Task Login_ReturnsPatientIdClaim_AfterPatientRegistration()
    {
        // Arrange
        var email = $"claim-{Guid.NewGuid():N}@test.local";
        const string password = "ValidPassword1!";
        using var client = CreateClient();
        var registerRequest = new RegisterRequest(
            email,
            password,
            "Claim",
            "Patient",
            new DateTime(1991, 3, 20),
            "Male",
            "+970599444444");
        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Act
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var tokenResponse = await loginResponse.Content
            .ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(tokenResponse?.Token));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenResponse!.Token);
        var patientIdClaim = jwt.Claims.Single(claim =>
            claim.Type == CustomClaimTypes.PatientId);
        Assert.True(int.TryParse(patientIdClaim.Value, out var patientId));
        Assert.True(patientId > 0);
    }

    // Verifies the complete Admin assignment and Nurse resource-access workflow.
    [Fact]
    public async Task Nurse_CanAccessAssignedPatient_ButCannotAccessOtherPatient()
    {
        // Arrange
        var suffix = Guid.NewGuid().ToString("N");
        using var adminClient = CreateAuthenticatedClient("admin-user", "Admin");
        var nurseRequest = new CreateNurseRequest(
            $"nurse-{suffix}@test.local",
            "ValidPassword1!",
            "Demo Cardiac Nurse",
            $"RN-{suffix[..8]}",
            "Cardiology");
        var nurseResponse = await adminClient.PostAsJsonAsync(
            "/api/staff/nurses",
            nurseRequest);
        Assert.Equal(HttpStatusCode.Created, nurseResponse.StatusCode);
        var nurse = await nurseResponse.Content.ReadFromJsonAsync<NurseResponse>();
        Assert.NotNull(nurse);

        var assignmentResponse = await adminClient.PostAsJsonAsync(
            "/api/patients/1/care-assignments",
            new CreateCareAssignmentRequest(
                nurse.Id,
                "Responsible for daily vital-sign monitoring."));
        Assert.Equal(HttpStatusCode.Created, assignmentResponse.StatusCode);

        using var nurseClient = CreateAuthenticatedClient(nurse.UserId, "Nurse");

        // Act
        var assignedPatientResponse = await nurseClient.GetAsync("/api/patients/1");
        var otherPatientResponse = await nurseClient.GetAsync("/api/patients/2");
        var myPatientsResponse = await nurseClient.GetAsync("/api/nurses/me/patients");
        var createNurseResponse = await nurseClient.PostAsJsonAsync(
            "/api/staff/nurses",
            nurseRequest with { Email = $"blocked-{suffix}@test.local" });

        // Assert
        Assert.Equal(HttpStatusCode.OK, assignedPatientResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, otherPatientResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, myPatientsResponse.StatusCode);
        var assignedPatients = await myPatientsResponse.Content
            .ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Contains(assignedPatients!, patient => patient.Id == 1);
        Assert.Equal(HttpStatusCode.Forbidden, createNurseResponse.StatusCode);
    }

    // Verifies Doctor creation, weekly availability, and appointment enforcement end to end.
    [Fact]
    public async Task DoctorSchedule_AllowsInsideAppointment_AndRejectsOutsideAppointment()
    {
        // Arrange
        var suffix = Guid.NewGuid().ToString("N");
        using var adminClient = CreateAuthenticatedClient("admin-user", "Admin");
        var doctorResponse = await adminClient.PostAsJsonAsync(
            "/api/staff/doctors",
            new CreateDoctorRequest(
                $"doctor-{suffix}@test.local",
                "ValidPassword1!",
                "Dr. Schedule Demo",
                $"MD-{suffix[..8]}",
                "Cardiology",
                "Cardiac Care"));
        Assert.Equal(HttpStatusCode.Created, doctorResponse.StatusCode);
        var doctor = await doctorResponse.Content.ReadFromJsonAsync<DoctorResponse>();
        Assert.NotNull(doctor);

        var appointmentDay = DateTime.UtcNow.Date.AddDays(1);
        var availabilityResponse = await adminClient.PostAsJsonAsync(
            $"/api/doctors/{doctor.Id}/availability",
            new CreateDoctorAvailabilityRequest(
                appointmentDay.DayOfWeek,
                new TimeOnly(9, 0),
                new TimeOnly(12, 0)));
        Assert.Equal(HttpStatusCode.Created, availabilityResponse.StatusCode);

        var insideRequest = new CreateAppointmentRequest(
            doctor.UserId,
            appointmentDay.AddHours(10),
            "Scheduled",
            "Inside weekly availability");
        var outsideRequest = insideRequest with
        {
            AppointmentDate = appointmentDay.AddHours(14),
            Notes = "Outside weekly availability"
        };

        // Act
        var insideResponse = await adminClient.PostAsJsonAsync(
            "/api/patients/1/appointments",
            insideRequest);
        var outsideResponse = await adminClient.PostAsJsonAsync(
            "/api/patients/1/appointments",
            outsideRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, insideResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, outsideResponse.StatusCode);
        var problem = await outsideResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Contains("weekly availability", problem?.Detail);
    }

    // Verifies that a critical reading produces an auditable alert workflow.
    [Fact]
    public async Task CriticalVitalSign_CreatesAlert_ThatCanBeAcknowledgedAndResolved()
    {
        // Arrange
        using var adminClient = CreateAuthenticatedClient("admin-user", "Admin");
        var vitalResponse = await adminClient.PostAsJsonAsync(
            "/api/patients/2/vitals",
            new CreateVitalSignRequest(190, 82m, 210, 125));
        Assert.Equal(HttpStatusCode.Created, vitalResponse.StatusCode);
        var vital = await vitalResponse.Content.ReadFromJsonAsync<VitalSignResponse>();
        Assert.NotNull(vital);

        var alertsResponse = await adminClient.GetAsync("/api/patients/2/alerts");
        Assert.Equal(HttpStatusCode.OK, alertsResponse.StatusCode);
        var alerts = await alertsResponse.Content
            .ReadFromJsonAsync<List<MedicalAlertResponse>>();
        var alert = Assert.Single(alerts!, item => item.VitalSignId == vital.Id);
        Assert.Equal("Critical", alert.Severity);
        Assert.Equal("Open", alert.Status);

        // Act
        var acknowledgeResponse = await adminClient.PatchAsync(
            $"/api/alerts/{alert.Id}/acknowledge",
            null);
        var resolveResponse = await adminClient.PatchAsync(
            $"/api/alerts/{alert.Id}/resolve",
            null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, acknowledgeResponse.StatusCode);
        var acknowledged = await acknowledgeResponse.Content
            .ReadFromJsonAsync<MedicalAlertResponse>();
        Assert.Equal("Acknowledged", acknowledged?.Status);
        Assert.Equal("admin-user", acknowledged?.AcknowledgedByUserId);

        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var resolved = await resolveResponse.Content
            .ReadFromJsonAsync<MedicalAlertResponse>();
        Assert.Equal("Resolved", resolved?.Status);
        Assert.Equal("admin-user", resolved?.ResolvedByUserId);
    }

    // Verifies that custom middleware preserves a client correlation ID.
    [Fact]
    public async Task RequestCorrelationMiddleware_ReturnsSuppliedCorrelationId()
    {
        // Arrange
        const string correlationId = "week7-demo-correlation-id";
        using var client = CreateAuthenticatedClient("admin-user", "Admin");
        client.DefaultRequestHeaders.Add(
            RequestCorrelationMiddleware.HeaderName,
            correlationId);

        // Act
        var response = await client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(
            RequestCorrelationMiddleware.HeaderName,
            out var values));
        Assert.Equal(correlationId, values.Single());
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
            .Setup(service => service.GetAllPatientsAsync(
                It.IsAny<PatientQueryParameters>()))
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
