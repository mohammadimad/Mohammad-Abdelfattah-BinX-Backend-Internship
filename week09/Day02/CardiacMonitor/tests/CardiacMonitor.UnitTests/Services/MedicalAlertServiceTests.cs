using CardiacMonitor.Data;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.UnitTests.Services;

public class MedicalAlertServiceTests
{
    // Verifies that acknowledgement and resolution record the responsible users.
    [Fact]
    public async Task AlertWorkflow_RecordsAcknowledgementAndResolutionAudit()
    {
        // Arrange
        await using var context = CreateContext();
        context.Patients.Add(CreatePatient());
        context.Users.AddRange(
            new IdentityUser { Id = "nurse-user", UserName = "nurse@example.com" },
            new IdentityUser { Id = "doctor-user", UserName = "doctor@example.com" });
        context.MedicalAlerts.Add(new MedicalAlert
        {
            Id = 1,
            PatientId = 1,
            Severity = "High",
            Status = "Open",
            Message = "High demonstration threshold detected.",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new MedicalAlertService(context);

        // Act
        var acknowledged = await service.AcknowledgeAsync(1, "nurse-user");
        var resolved = await service.ResolveAsync(1, "doctor-user");

        // Assert
        Assert.True(acknowledged.Succeeded);
        Assert.Equal("Acknowledged", acknowledged.Alert?.Status);
        Assert.Equal("nurse-user", acknowledged.Alert?.AcknowledgedByUserId);
        Assert.True(resolved.Succeeded);
        Assert.Equal("Resolved", resolved.Alert?.Status);
        Assert.Equal("doctor-user", resolved.Alert?.ResolvedByUserId);
    }

    // Creates an isolated EF Core context for each alert test.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Creates the patient linked to a unit-test medical alert.
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
}
