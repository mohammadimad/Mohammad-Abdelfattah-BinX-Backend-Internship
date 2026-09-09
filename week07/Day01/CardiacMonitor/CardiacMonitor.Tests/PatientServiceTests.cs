using CardiacMonitor.Data;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CardiacMonitor.Tests.Services;

public class PatientServiceTests
{
    // Verifies an existing patient is projected to a response DTO.
    [Fact]
    public async Task GetPatientByIdAsync_WhenPatientExists_ReturnsMappedPatient()
    {
        await using var context = CreateContext();
        var patient = CreatePatient(1);
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        var service = new PatientService(context);

        var result = await service.GetPatientByIdAsync(patient.Id);

        Assert.NotNull(result);
        Assert.Equal(patient.Id, result.Id);
        Assert.Equal(patient.FirstName, result.FirstName);
        Assert.Equal(patient.LastName, result.LastName);
    }

    // Verifies a missing patient returns null.
    [Fact]
    public async Task GetPatientByIdAsync_WhenPatientDoesNotExist_ReturnsNull()
    {
        await using var context = CreateContext();
        var service = new PatientService(context);

        var result = await service.GetPatientByIdAsync(999);

        Assert.Null(result);
    }

    // Creates an isolated in-memory database context for a test.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Creates a valid patient test entity.
    private static Patient CreatePatient(int id) => new()
    {
        Id = id,
        FirstName = "Ahmad",
        LastName = "Amr",
        DateOfBirth = new DateTime(1990, 5, 12),
        Gender = "Male",
        ContactNumber = "+9759835279"
    };
}
