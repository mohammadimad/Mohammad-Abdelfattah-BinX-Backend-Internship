using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CardiacMonitor.Tests.Integration;

public class SprintOneApiTests : IClassFixture<CardiacMonitorWebApplicationFactory>
{
    private readonly CardiacMonitorWebApplicationFactory _factory;

    public SprintOneApiTests(CardiacMonitorWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
    }

    [Fact]
    public async Task GetPatients_WithPaginationFilteringAndSorting_ReturnsProjectedPage()
    {
        var client = await CreateDoctorClientAsync();

        var response = await client.GetAsync(
            "/api/patients?pageNumber=1&pageSize=1&searchName=a&gender=Female&sortBy=lastName&isDescending=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PaginatedList<PatientResponse>>();
        Assert.NotNull(page);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(1, page.PageSize);

        var patient = Assert.Single(page.Items);
        Assert.Equal("Sara", patient.FirstName);
        Assert.Equal("Ali", patient.LastName);
        Assert.Equal("Female", patient.Gender);
    }

    [Fact]
    public async Task GetPatients_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/patients");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateMedicationOrder_WithAvailableStock_ReturnsCreatedAndPersistsAllChanges()
    {
        await SeedMedicationAsync(stockQuantity: 5, unitPrice: 3.50m);
        var client = await CreateDoctorClientAsync();
        var request = new CreateMedicationOrderRequest(new[]
        {
            new CreateMedicationOrderItemRequest(100, 2)
        });

        var response = await client.PostAsJsonAsync(
            "/api/patients/1/medication-orders",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<MedicationOrderResponse>();
        Assert.NotNull(order);
        Assert.Equal(7.00m, order.TotalAmount);
        Assert.Equal(7.00m, Assert.Single(order.Items).LineTotal);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(3, (await context.Medications.FindAsync(100))!.StockQuantity);
        Assert.Single(await context.MedicationOrders.ToListAsync());
        Assert.Single(await context.MedicationOrderItems.ToListAsync());
    }

    [Fact]
    public async Task CreateMedicationOrder_WithInsufficientStock_ReturnsConflictWithoutChanges()
    {
        await SeedMedicationAsync(stockQuantity: 1, unitPrice: 3.50m);
        var client = await CreateDoctorClientAsync();
        var request = new CreateMedicationOrderRequest(new[]
        {
            new CreateMedicationOrderItemRequest(100, 2)
        });

        var response = await client.PostAsJsonAsync(
            "/api/patients/1/medication-orders",
            request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, (await context.Medications.FindAsync(100))!.StockQuantity);
        Assert.Empty(await context.MedicationOrders.ToListAsync());
        Assert.Empty(await context.MedicationOrderItems.ToListAsync());
    }

    private async Task<HttpClient> CreateDoctorClientAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("doctor@cardiac.com", "Doctor@123"));
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    private async Task SeedMedicationAsync(int stockQuantity, decimal unitPrice)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Medications.Add(new Medication
        {
            Id = 100,
            PatientId = 1,
            Name = "Demo Aspirin",
            Dosage = "81 mg",
            Frequency = "Once daily",
            StartDate = new DateTime(2026, 1, 1),
            IsActive = true,
            StockQuantity = stockQuantity,
            UnitPrice = unitPrice
        });
        await context.SaveChangesAsync();
    }
}
