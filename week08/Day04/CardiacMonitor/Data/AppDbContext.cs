
using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<NurseProfile> NurseProfiles => Set<NurseProfile>();
    public DbSet<PatientCareAssignment> PatientCareAssignments =>
        Set<PatientCareAssignment>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<DoctorAvailability> DoctorAvailabilitySlots =>
        Set<DoctorAvailability>();
    public DbSet<MedicalAlert> MedicalAlerts => Set<MedicalAlert>();

    // 🛠️ التابع المسؤول عن إعداد سلوك الـ DbContext
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        //   Enable SQL query printing in the console when running in the development environment.
#if DEBUG
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)
                              .EnableSensitiveDataLogging();
        #endif
    }

    // Configures the database model and applies deterministic seed data.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData.Configure(builder);

        // 1. Composite index for accelerating the reading and history of a patient's vital signs.
        builder.Entity<VitalSign>()
       .HasIndex(v => new { v.PatientId, v.RecordedAt })
       .HasDatabaseName("IX_VitalSigns_PatientId_RecordedAt");

        // 2. A composite index to accelerate the filtering and sorting of the patient catalog.
        builder.Entity<Patient>()
            .HasIndex(p => new { p.Gender, p.LastName })
            .HasDatabaseName("IX_Patients_Gender_LastName");

        // 3.A unique composite index to prevent scheduling conflicts for doctors and accelerate search processes.  
        builder.Entity<Appointment>()
            .HasIndex(a => new { a.DoctorId, a.AppointmentDate })
            .IsUnique()
            .HasDatabaseName("UX_Appointments_DoctorId_AppointmentDate");
    }
}
