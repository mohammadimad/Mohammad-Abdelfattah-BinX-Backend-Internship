using System.Security.Claims;
using CardiacMonitor.Data;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.UnitTests.Services;

public class PatientAccessServiceTests
{
    // Verifies that a Nurse can access only a patient with an active assignment.
    [Fact]
    public async Task CanAccessPatientAsync_AllowsOnlyActivelyAssignedNurse()
    {
        // Arrange
        await using var context = CreateContext();
        var nurseUser = new IdentityUser
        {
            Id = "nurse-user",
            UserName = "nurse@test.local",
            Email = "nurse@test.local"
        };
        var assignedByUser = new IdentityUser
        {
            Id = "admin-user",
            UserName = "admin@test.local",
            Email = "admin@test.local"
        };
        var assignedPatient = CreatePatient(101, "Assigned");
        var unassignedPatient = CreatePatient(102, "Unassigned");
        var nurse = new NurseProfile
        {
            Id = 11,
            UserId = nurseUser.Id,
            FullName = "Nurse Demo",
            LicenseNumber = "RN-101",
            Department = "Cardiology"
        };

        context.Users.AddRange(nurseUser, assignedByUser);
        context.Patients.AddRange(assignedPatient, unassignedPatient);
        context.NurseProfiles.Add(nurse);
        context.PatientCareAssignments.Add(new PatientCareAssignment
        {
            PatientId = assignedPatient.Id,
            NurseProfileId = nurse.Id,
            AssignedByUserId = assignedByUser.Id,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new PatientAccessService(context);
        var user = CreateUser("nurse-user", "Nurse");

        // Act
        var assignedAccess = await service.CanAccessPatientAsync(
            user,
            assignedPatient.Id);
        var unassignedAccess = await service.CanAccessPatientAsync(
            user,
            unassignedPatient.Id);

        // Assert
        Assert.True(assignedAccess);
        Assert.False(unassignedAccess);
    }

    // Creates an isolated EF Core context for access-control tests.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // Creates a valid patient for assignment access tests.
    private static Patient CreatePatient(int id, string firstName)
    {
        return new Patient
        {
            Id = id,
            FirstName = firstName,
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male",
            ContactNumber = $"+97059900{id}"
        };
    }

    // Creates an authenticated principal with the selected identity and role.
    private static ClaimsPrincipal CreateUser(string userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}
