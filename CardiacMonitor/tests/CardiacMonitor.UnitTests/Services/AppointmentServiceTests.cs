using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.UnitTests.Services;

public class AppointmentServiceTests
{
    // Verifies that an ordinary identity user cannot be assigned as a doctor.
    [Fact]
    public async Task CreateAppointmentAsync_ReturnsNull_WhenUserIsNotInDoctorRole()
    {
        // Arrange
        await using var context = CreateContext();
        context.Patients.Add(CreatePatient());
        context.Users.Add(new IdentityUser
        {
            Id = "ordinary-user",
            UserName = "ordinary@example.com"
        });
        await context.SaveChangesAsync();

        var service = new AppointmentService(context);
        var request = new CreateAppointmentRequest(
            "ordinary-user",
            DateTime.UtcNow.AddDays(1),
            "Scheduled",
            null);

        // Act
        var result = await service.CreateAppointmentAsync(1, request);

        // Assert
        Assert.Null(result);
        Assert.Empty(context.Appointments);
    }

    // Verifies that a user in the Doctor role can be assigned to an appointment.
    [Fact]
    public async Task CreateAppointmentAsync_CreatesAppointment_WhenUserIsInDoctorRole()
    {
        // Arrange
        await using var context = CreateContext();
        var doctorRole = new IdentityRole
        {
            Id = "doctor-role",
            Name = "Doctor",
            NormalizedName = "DOCTOR"
        };
        var doctor = new IdentityUser
        {
            Id = "doctor-user",
            UserName = "doctor@example.com"
        };

        context.Patients.Add(CreatePatient());
        context.Roles.Add(doctorRole);
        context.Users.Add(doctor);
        context.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = doctor.Id,
            RoleId = doctorRole.Id
        });
        await context.SaveChangesAsync();

        var service = new AppointmentService(context);
        var request = new CreateAppointmentRequest(
            doctor.Id,
            DateTime.UtcNow.AddDays(1),
            "Scheduled",
            "Unit test appointment");

        // Act
        var result = await service.CreateAppointmentAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(doctor.Id, result.DoctorId);
        Assert.Single(context.Appointments);
    }

    // Creates an isolated in-memory database context for each unit test.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Creates a valid patient used by appointment service tests.
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
