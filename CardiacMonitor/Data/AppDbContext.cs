
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

    // Configures the database model and applies deterministic seed data.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData.Configure(builder);
    }
}
