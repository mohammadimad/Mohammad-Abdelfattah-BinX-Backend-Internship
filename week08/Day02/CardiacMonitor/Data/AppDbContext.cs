
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
    }
}
