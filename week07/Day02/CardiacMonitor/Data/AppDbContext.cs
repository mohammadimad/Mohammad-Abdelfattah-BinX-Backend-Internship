
using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CardiacMonitor.Data
{

    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        // Initializes the application database context.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {}

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
        public DbSet<Medication> Medications => Set<Medication>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<MedicationOrder> MedicationOrders => Set<MedicationOrder>();
        public DbSet<MedicationOrderItem> MedicationOrderItems => Set<MedicationOrderItem>();

        // Configures entity relationships, constraints, and seed data.
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

            // Configure medication price and stock constraints
            builder.Entity<Medication>()
                .Property(m => m.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<Medication>()
                .ToTable(table => table.HasCheckConstraint("CK_Medications_StockQuantity", "[StockQuantity] >= 0"));

            // Configure medication order relationships and monetary precision
            builder.Entity<MedicationOrder>()
                .HasOne(order => order.Patient)
                .WithMany()
                .HasForeignKey(order => order.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MedicationOrder>()
                .Property(order => order.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<MedicationOrderItem>()
                .HasOne(item => item.MedicationOrder)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.MedicationOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MedicationOrderItem>()
                .HasOne(item => item.Medication)
                .WithMany(medication => medication.OrderItems)
                .HasForeignKey(item => item.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MedicationOrderItem>()
                .Property(item => item.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<MedicationOrderItem>()
                .Property(item => item.LineTotal)
                .HasPrecision(18, 2);

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
            var doctorUser = new IdentityUser
            {
                Id = doctorUserId,
                UserName = "doctor@cardiac.com",
                NormalizedUserName = "DOCTOR@CARDIAC.COM",
                Email = "doctor@cardiac.com",
                NormalizedEmail = "DOCTOR@CARDIAC.COM",
                EmailConfirmed = true,
                ConcurrencyStamp = "39143f1f-32af-417e-a9bb-4677a98a05d4",
                SecurityStamp = "751dbe44-2075-46c7-8161-ca9b538ddb3f",
                PasswordHash = "AQAAAAIAAYagAAAAEBL5yogH/gmLUuVjFuqsUXxhg44x5V+Bm0k68Imyx0vwy0S57ZiBNc/X1G1prBobvA=="
            };

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
                    RecordedAt = new DateTime(2026, 8, 15, 15, 45, 5, 747, DateTimeKind.Utc).AddTicks(6804)
                },
                new VitalSign
                {
                    Id = 2,
                    PatientId = 1,
                    HeartRate = 82,
                    OxygenSaturation = 97.0m,
                    SystolicBP = 125,
                    DiastolicBP = 82,
                    RecordedAt = new DateTime(2026, 8, 15, 16, 45, 5, 747, DateTimeKind.Utc).AddTicks(6812)
                },
                new VitalSign
                {
                    Id = 3,
                    PatientId = 2,
                    HeartRate = 70,
                    OxygenSaturation = 99.0m,
                    SystolicBP = 115,
                    DiastolicBP = 75,
                    RecordedAt = new DateTime(2026, 8, 15, 17, 15, 5, 747, DateTimeKind.Utc).AddTicks(6814)
                }
            );
        }
    }
}
