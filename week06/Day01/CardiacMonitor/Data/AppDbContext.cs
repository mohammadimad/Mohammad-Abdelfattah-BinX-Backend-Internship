
using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CardiacMonitor.Data
{

    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {}

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
        public DbSet<Medication> Medications => Set<Medication>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //RfereshToken relationship with IdentityUser
            builder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
                    base.OnModelCreating(builder); 
            //1 to 1 relationship between Patient and IdentityUser
            builder.Entity<Patient>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); 
            //1 to Many relationship between Patient and VitalSign
            builder.Entity<VitalSign>()
                .HasOne(v => v.Patient)
                .WithMany(p => p.VitalSigns)
                .HasForeignKey(v => v.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set precision for decimal properties in VitalSign
            builder.Entity<VitalSign>()
                .Property(v => v.OxygenSaturation)
                .HasPrecision(5, 2);
            //1 to Many relationship between Patient and Medication
            builder.Entity<Medication>()
                .HasOne(m => m.Patient)
                .WithMany()
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1 to Many relationship between Appointment and Patient
            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1 to Many relationship between Appointment and Doctor 
            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data for IdentityUser

            // Seed roles
            var adminRoleId = "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d";
            var doctorRoleId = "b2c3d4e5-f67a-8b9c-0d1e-2f3a4b5c6d7e";
            var patientRoleId = "c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = doctorRoleId, Name = "Doctor", NormalizedName = "DOCTOR" },
                new IdentityRole { Id = patientRoleId, Name = "Patient", NormalizedName = "PATIENT" }
            );
           // Seed a doctor user
            var doctorUserId = "doctor-id-123"; 
            var hasher = new PasswordHasher<IdentityUser>();

            var doctorUser = new IdentityUser
            {
                Id = doctorUserId,
                UserName = "doctor@cardiac.com",
                NormalizedUserName = "DOCTOR@CARDIAC.COM",
                Email = "doctor@cardiac.com",
                NormalizedEmail = "DOCTOR@CARDIAC.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            doctorUser.PasswordHash = hasher.HashPassword(doctorUser, "Doctor@123");

            builder.Entity<IdentityUser>().HasData(doctorUser);

            // Assign the doctor user to the Doctor role
            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = doctorRoleId,
                UserId = doctorUserId
            });
            //Patient data for seeding
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
                }
            );

           // Vital signs for the patients
            builder.Entity<VitalSign>().HasData(
                new VitalSign
                {
                    Id = 1,
                    PatientId = 1,
                    HeartRate = 75,
                    OxygenSaturation = 98.5m,
                    SystolicBP = 120,
                    DiastolicBP = 80,
                    RecordedAt = DateTime.UtcNow.AddHours(-2)
                },
                new VitalSign
                {
                    Id = 2,
                    PatientId = 1,
                    HeartRate = 82,
                    OxygenSaturation = 97.0m,
                    SystolicBP = 125,
                    DiastolicBP = 82,
                    RecordedAt = DateTime.UtcNow.AddHours(-1)
                },
                new VitalSign
                {
                    Id = 3,
                    PatientId = 2,
                    HeartRate = 70,
                    OxygenSaturation = 99.0m,
                    SystolicBP = 115,
                    DiastolicBP = 75,
                    RecordedAt = DateTime.UtcNow.AddMinutes(-30)
                }
            );
        }
    }
}
