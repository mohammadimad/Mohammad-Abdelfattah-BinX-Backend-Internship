using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.UnitTests.Services;

public class CareAssignmentServiceTests
{
    // Verifies that ending an assignment removes the patient's Nurse access.
    [Fact]
    public async Task EndAssignmentAsync_RemovesPatientFromActiveNurseList()
    {
        // Arrange
        await using var context = CreateContext();
        var nurseUser = CreateUser("nurse-user", "nurse@test.local");
        var adminUser = CreateUser("admin-user", "admin@test.local");
        var patient = CreatePatient();
        var nurse = new NurseProfile
        {
            Id = 7,
            UserId = nurseUser.Id,
            FullName = "Nurse Demo",
            LicenseNumber = "RN-700",
            Department = "Cardiology"
        };
        context.Users.AddRange(nurseUser, adminUser);
        context.Patients.Add(patient);
        context.NurseProfiles.Add(nurse);
        await context.SaveChangesAsync();

        var service = new CareAssignmentService(context);
        var result = await service.AssignNurseAsync(
            patient.Id,
            new CreateCareAssignmentRequest(nurse.Id, "Morning monitoring"),
            adminUser.Id);

        // Act
        var ended = await service.EndAssignmentAsync(result.Assignment!.Id);
        var assignedPatients = await service.GetAssignedPatientsAsync(
            nurseUser.Id);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(ended);
        Assert.Empty(assignedPatients);
        var storedAssignment = await context.PatientCareAssignments.SingleAsync();
        Assert.False(storedAssignment.IsActive);
        Assert.NotNull(storedAssignment.EndedAt);
    }

    // Creates an isolated EF Core context for care-assignment tests.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // Creates an Identity user for a care-assignment test.
    private static IdentityUser CreateUser(string id, string email)
    {
        return new IdentityUser
        {
            Id = id,
            UserName = email,
            Email = email
        };
    }

    // Creates the patient used by a care-assignment test.
    private static Patient CreatePatient()
    {
        return new Patient
        {
            Id = 201,
            FirstName = "Care",
            LastName = "Patient",
            DateOfBirth = new DateTime(1988, 4, 10),
            Gender = "Female",
            ContactNumber = "+970599333333"
        };
    }
}
