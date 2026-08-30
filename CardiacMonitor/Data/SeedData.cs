using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Data;

public static class SeedData
{
    public const string AdminRoleId = "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d";
    public const string DoctorRoleId = "b2c3d4e5-f67a-8b9c-0d1e-2f3a4b5c6d7e";
    public const string PatientRoleId = "c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f";
    public const string DoctorUserId = "doctor-id-123";

    // Adds stable reference and demonstration data to the EF Core model.
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = AdminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = DoctorRoleId, Name = "Doctor", NormalizedName = "DOCTOR" },
            new IdentityRole { Id = PatientRoleId, Name = "Patient", NormalizedName = "PATIENT" });

        builder.Entity<IdentityUser>().HasData(new IdentityUser
        {
            Id = DoctorUserId,
            UserName = "doctor@cardiac.com",
            NormalizedUserName = "DOCTOR@CARDIAC.COM",
            Email = "doctor@cardiac.com",
            NormalizedEmail = "DOCTOR@CARDIAC.COM",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAEBL5yogH/gmLUuVjFuqsUXxhg44x5V+Bm0k68Imyx0vwy0S57ZiBNc/X1G1prBobvA==",
            SecurityStamp = "751dbe44-2075-46c7-8161-ca9b538ddb3f",
            ConcurrencyStamp = "39143f1f-32af-417e-a9bb-4677a98a05d4"
        });

        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = DoctorRoleId,
            UserId = DoctorUserId
        });

        builder.Entity<Patient>().HasData(
            new Patient
            {
                Id = 1,
                FirstName = "Ahmad",
                LastName = "Amr",
                DateOfBirth = new DateTime(1990, 5, 12),
                Gender = "Male",
                ContactNumber = "+9759835279",
                UserId = null
            },
            new Patient
            {
                Id = 2,
                FirstName = "Sara",
                LastName = "Ali",
                DateOfBirth = new DateTime(1985, 10, 22),
                Gender = "Female",
                ContactNumber = "+970988271",
                UserId = null
            });

        builder.Entity<VitalSign>().HasData(
            new VitalSign
            {
                Id = 1,
                PatientId = 1,
                HeartRate = 75,
                OxygenSaturation = 98.5m,
                SystolicBP = 120,
                DiastolicBP = 80,
                RecordedAt = new DateTime(2026, 8, 15, 15, 0, 0, DateTimeKind.Utc)
            },
            new VitalSign
            {
                Id = 2,
                PatientId = 1,
                HeartRate = 82,
                OxygenSaturation = 97.0m,
                SystolicBP = 125,
                DiastolicBP = 82,
                RecordedAt = new DateTime(2026, 8, 15, 16, 0, 0, DateTimeKind.Utc)
            },
            new VitalSign
            {
                Id = 3,
                PatientId = 2,
                HeartRate = 70,
                OxygenSaturation = 99.0m,
                SystolicBP = 115,
                DiastolicBP = 75,
                RecordedAt = new DateTime(2026, 8, 15, 16, 30, 0, DateTimeKind.Utc)
            });
    }
}
