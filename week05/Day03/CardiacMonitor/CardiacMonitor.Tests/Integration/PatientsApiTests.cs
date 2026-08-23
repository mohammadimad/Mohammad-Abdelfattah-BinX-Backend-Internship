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

// WebApplicationFactory يشغّل التطبيق كاملًا داخل الذاكرة ويعطينا HttpClient بدون فتح منفذ شبكة حقيقي.
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
            // نستبدل SQL Server بقاعدة InMemory منفصلة حتى لا تغيّر الاختبارات بيانات التطوير.
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

        // إعادة إنشاء القاعدة تجعل كل اختبار مستقلاً وقابلاً للتكرار بأي ترتيب.
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

    // نحصل على JWT حقيقي من endpoint تسجيل الدخول كما تطلب المادة لاختبار endpoint محمي.
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

        // Act: طلب HTTP حقيقي يمر عبر routing وmiddleware وDI.
        var response = await client.GetAsync($"/api/patients/{patientId}");

        // Assert: نفحص الحالة وكامل بيانات الاستجابة المطلوبة في مختبر Day03.
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

        // Assert: هذا هو مسار الخطأ لنفس endpoint.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPatientById_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var patientId = 1;

        // Act: لا نضيف Authorization header عمدًا.
        var response = await client.GetAsync($"/api/patients/{patientId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
