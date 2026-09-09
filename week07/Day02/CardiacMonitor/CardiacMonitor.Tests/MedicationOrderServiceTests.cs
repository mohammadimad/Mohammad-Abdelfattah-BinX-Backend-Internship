using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CardiacMonitor.Tests;

public class MedicationOrderServiceTests
{
    // Verifies totals, order persistence, and stock updates for a valid order.
    [Fact]
    public async Task CreateOrderAsync_WithAvailableStock_CreatesOrderAndDecrementsStock()
    {
        await using var context = CreateContext();
        context.Patients.Add(new Patient { Id = 10, FirstName = "Test", LastName = "Patient" });
        context.Medications.AddRange(
            new Medication { Id = 1, PatientId = 10, Name = "Aspirin", IsActive = true, StockQuantity = 10, UnitPrice = 2.50m },
            new Medication { Id = 2, PatientId = 10, Name = "Statin", IsActive = true, StockQuantity = 5, UnitPrice = 7.00m });
        await context.SaveChangesAsync();

        var service = new MedicationOrderService(context);
        var request = new CreateMedicationOrderRequest(new[]
        {
            new CreateMedicationOrderItemRequest(1, 2),
            new CreateMedicationOrderItemRequest(2, 1)
        });

        var result = await service.CreateOrderAsync(10, request);

        Assert.Equal(CreateMedicationOrderStatus.Created, result.Status);
        Assert.NotNull(result.Order);
        Assert.Equal(12.00m, result.Order.TotalAmount);
        Assert.Equal(new[] { 5.00m, 7.00m }, result.Order.Items.Select(item => item.LineTotal));
        Assert.Equal(8, (await context.Medications.FindAsync(1))!.StockQuantity);
        Assert.Equal(4, (await context.Medications.FindAsync(2))!.StockQuantity);
        Assert.Single(context.MedicationOrders);
        Assert.Equal(2, context.MedicationOrderItems.Count());
    }

    // Verifies insufficient stock leaves the database unchanged.
    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_RejectsWithoutChanges()
    {
        await using var context = CreateContext();
        context.Patients.Add(new Patient { Id = 20, FirstName = "Test", LastName = "Patient" });
        context.Medications.Add(
            new Medication { Id = 3, PatientId = 20, Name = "Beta Blocker", IsActive = true, StockQuantity = 1, UnitPrice = 4.00m });
        await context.SaveChangesAsync();

        var service = new MedicationOrderService(context);
        var request = new CreateMedicationOrderRequest(new[]
        {
            new CreateMedicationOrderItemRequest(3, 2)
        });

        var result = await service.CreateOrderAsync(20, request);

        Assert.Equal(CreateMedicationOrderStatus.InsufficientStock, result.Status);
        Assert.Null(result.Order);
        Assert.Equal(1, (await context.Medications.FindAsync(3))!.StockQuantity);
        Assert.Empty(context.MedicationOrders);
        Assert.Empty(context.MedicationOrderItems);
    }

    // Creates an isolated in-memory database context for a test.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
