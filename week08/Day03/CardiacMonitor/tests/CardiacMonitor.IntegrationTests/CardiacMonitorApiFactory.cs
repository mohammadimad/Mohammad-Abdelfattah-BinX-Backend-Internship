using CardiacMonitor.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CardiacMonitor.IntegrationTests;

public sealed class CardiacMonitorApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    // Replaces SQL Server with an isolated SQLite database for integration tests.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });

        builder.ConfigureServices(services =>
        {
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            var databaseConfigurations = services
                .Where(descriptor => descriptor.ServiceType.Name.StartsWith(
                    "IDbContextOptionsConfiguration",
                    StringComparison.Ordinal))
                .ToList();

            foreach (var databaseConfiguration in databaseConfigurations)
            {
                services.Remove(databaseConfiguration);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    // Starts the test host and seeds identities required by authorization tests.
    protected override IHost CreateHost(IHostBuilder builder)
    {
        _connection.Open();
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        SeedTestData(context);

        return host;
    }

    // Releases the in-memory SQLite connection after the test suite finishes.
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }

    // Adds deterministic users and links a patient profile to its test identity.
    private static void SeedTestData(AppDbContext context)
    {
        if (context.Users.Any(user => user.Id == "admin-user"))
        {
            return;
        }

        var admin = new IdentityUser
        {
            Id = "admin-user",
            UserName = "admin@test.local",
            NormalizedUserName = "ADMIN@TEST.LOCAL",
            Email = "admin@test.local",
            NormalizedEmail = "ADMIN@TEST.LOCAL"
        };
        var patientUser = new IdentityUser
        {
            Id = "patient-user",
            UserName = "patient@test.local",
            NormalizedUserName = "PATIENT@TEST.LOCAL",
            Email = "patient@test.local",
            NormalizedEmail = "PATIENT@TEST.LOCAL"
        };

        var adminRole = context.Roles.Single(role => role.NormalizedName == "ADMIN");
        var patientRole = context.Roles.Single(role => role.NormalizedName == "PATIENT");
        var patient = context.Patients.Single(entity => entity.Id == 1);

        context.Users.AddRange(admin, patientUser);
        context.UserRoles.AddRange(
            new IdentityUserRole<string>
            {
                UserId = admin.Id,
                RoleId = adminRole.Id
            },
            new IdentityUserRole<string>
            {
                UserId = patientUser.Id,
                RoleId = patientRole.Id
            });

        patient.UserId = patientUser.Id;
        context.SaveChanges();
    }
}
