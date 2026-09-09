using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.UnitTests.Services;

public class VitalSignServiceTests
{
    // Verifies that vital-sign filters, sorting, and pagination work together.
    [Fact]
    public async Task GetVitalSignsAsync_ReturnsRequestedFilteredPage()
    {
        // Arrange
        await using var context = CreateContext();
        context.Patients.Add(CreatePatient());
        context.VitalSigns.AddRange(
            CreateVitalSign(1, 65, new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc)),
            CreateVitalSign(2, 90, new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc)),
            CreateVitalSign(3, 110, new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc)));
        await context.SaveChangesAsync();

        var service = new VitalSignService(context);
        var query = new VitalSignQueryParameters(
            Page: 1,
            PageSize: 1,
            MinHeartRate: 80,
            Sort: "heartRate_desc");

        // Act
        var result = await service.GetVitalSignsByPatientIdAsync(1, query);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(110, result.Items[0].HeartRate);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    // Verifies that a critical reading creates a linked open medical alert.
    [Fact]
    public async Task CreateVitalSignAsync_CreatesCriticalAlert_WhenThresholdIsExceeded()
    {
        // Arrange
        await using var context = CreateContext();
        context.Patients.Add(CreatePatient());
        await context.SaveChangesAsync();

        var service = new VitalSignService(context);
        var request = new CreateVitalSignRequest(
            HeartRate: 190,
            OxygenSaturation: 82,
            SystolicBP: 210,
            DiastolicBP: 125);

        // Act
        var created = await service.CreateVitalSignAsync(1, request);

        // Assert
        Assert.NotNull(created);
        var alert = await context.MedicalAlerts.SingleAsync();
        Assert.Equal(created.Id, alert.VitalSignId);
        Assert.Equal("Critical", alert.Severity);
        Assert.Equal("Open", alert.Status);
        Assert.Contains("heart rate 190 bpm", alert.Message);
    }

    // Creates an isolated EF Core context for a vital-sign service test.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Creates the patient linked to unit-test vital signs.
    private static Patient CreatePatient()
    {
        return new Patient
        {
            Id = 1,
            FirstName = "Demo",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            ContactNumber = "+970599123456"
        };
    }

    // Creates a valid unit-test vital-sign record.
    private static VitalSign CreateVitalSign(int id, int heartRate, DateTime recordedAt)
    {
        return new VitalSign
        {
            Id = id,
            PatientId = 1,
            HeartRate = heartRate,
            OxygenSaturation = 98,
            SystolicBP = 120,
            DiastolicBP = 80,
            RecordedAt = recordedAt
        };
    }
}
