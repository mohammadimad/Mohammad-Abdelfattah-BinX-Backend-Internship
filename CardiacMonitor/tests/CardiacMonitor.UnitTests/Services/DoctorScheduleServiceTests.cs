using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CardiacMonitor.UnitTests.Services;

public class DoctorScheduleServiceTests
{
    // Verifies that overlapping active availability slots are rejected.
    [Fact]
    public async Task AddAvailabilityAsync_RejectsOverlappingSlot()
    {
        // Arrange
        await using var context = CreateContext();
        AddDoctor(context);
        context.DoctorAvailabilitySlots.Add(new DoctorAvailability
        {
            DoctorProfileId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        var service = new DoctorScheduleService(context);
        var request = new CreateDoctorAvailabilityRequest(
            DayOfWeek.Monday,
            new TimeOnly(11, 0),
            new TimeOnly(13, 0));

        // Act
        var result = await service.AddAvailabilityAsync(1, request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("overlaps", result.Message);
        Assert.Single(context.DoctorAvailabilitySlots);
    }

    // Verifies that appointment time validation follows the Doctor weekly schedule.
    [Fact]
    public async Task IsDoctorAvailableAsync_ReturnsExpectedScheduleResult()
    {
        // Arrange
        await using var context = CreateContext();
        AddDoctor(context);
        context.DoctorAvailabilitySlots.Add(new DoctorAvailability
        {
            DoctorProfileId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        var service = new DoctorScheduleService(context);

        // Act
        var insideSchedule = await service.IsDoctorAvailableAsync(
            "doctor-user",
            new DateTime(2026, 9, 7, 10, 0, 0, DateTimeKind.Utc));
        var outsideSchedule = await service.IsDoctorAvailableAsync(
            "doctor-user",
            new DateTime(2026, 9, 7, 13, 0, 0, DateTimeKind.Utc));

        // Assert
        Assert.True(insideSchedule);
        Assert.False(outsideSchedule);
    }

    // Verifies cache-aside reads and immediate invalidation after a schedule write.
    [Fact]
    public async Task GetAvailabilityAsync_RefreshesCacheAfterAvailabilityIsAdded()
    {
        // Arrange
        await using var context = CreateContext();
        AddDoctor(context);
        context.DoctorAvailabilitySlots.Add(new DoctorAvailability
        {
            DoctorProfileId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(12, 0)
        });
        await context.SaveChangesAsync();

        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<
            Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
        var service = new DoctorScheduleService(context, cache);

        // Act
        var initialAvailability = await service.GetAvailabilityAsync(1);
        var addResult = await service.AddAvailabilityAsync(
            1,
            new CreateDoctorAvailabilityRequest(
                DayOfWeek.Tuesday,
                new TimeOnly(13, 0),
                new TimeOnly(16, 0)));
        var refreshedAvailability = await service.GetAvailabilityAsync(1);

        // Assert
        Assert.Single(initialAvailability);
        Assert.True(addResult.Succeeded);
        Assert.Equal(2, refreshedAvailability.Count);
    }

    // Creates an isolated EF Core context for each schedule test.
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Adds an active Doctor identity and profile to the test context.
    private static void AddDoctor(AppDbContext context)
    {
        var user = new IdentityUser
        {
            Id = "doctor-user",
            UserName = "doctor@example.com"
        };
        context.Users.Add(user);
        context.DoctorProfiles.Add(new DoctorProfile
        {
            Id = 1,
            UserId = user.Id,
            FullName = "Dr. Demo",
            LicenseNumber = "DOC-1001",
            Specialty = "Cardiology",
            Department = "Cardiology",
            IsActive = true
        });
    }
}
