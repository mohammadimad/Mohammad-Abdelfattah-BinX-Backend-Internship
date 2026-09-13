using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.UnitTests.Services;

public class PatientServiceTests
{
    // Verifies that patient search, gender filtering, sorting, and pagination work together.
    [Fact]
    public async Task GetAllPatientsAsync_ReturnsRequestedFilteredPage()
    {
        // Arrange
        await using var context = CreateContext();
        context.Patients.AddRange(
            CreatePatient(1, "Ali", "Zaid", "Male"),
            CreatePatient(2, "Alina", "Omar", "Female"),
            CreatePatient(3, "Khaled", "Ali", "Male"));
        await context.SaveChangesAsync();

        var service = new PatientService(context);
        var query = new PatientQueryParameters(
            Page: 1,
            PageSize: 1,
            Search: "ali",
            Gender: "male",
            Sort: "lastName_desc");

        // Act
        var result = await service.GetAllPatientsAsync(query);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Ali", result.Items[0].FirstName);
        Assert.Equal("Zaid", result.Items[0].LastName);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    // Creates an isolated EF Core context for a patient service test.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Creates a valid patient record for query tests.
    private static Patient CreatePatient(
        int id,
        string firstName,
        string lastName,
        string gender)
    {
        return new Patient
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = new DateTime(1990, 1, id),
            Gender = gender,
            ContactNumber = $"+97059900000{id}"
        };
    }
}
