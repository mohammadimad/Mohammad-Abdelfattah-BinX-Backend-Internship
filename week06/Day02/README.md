# Week 6 - Day 2: Building the EF Core Data Model & Migrations

## Day Overview

Day 2 focused on translating our finalized, normalized 3NF ERD into concrete C# entity classes using Entity Framework Core [4, 5]. We established our database context (`AppDbContext`), configured explicit relationships and delete behaviors using the Fluent API, seeded initial system and reference data, and successfully generated and applied our migrations to SQL Server [5].

## What We Learned

- How to map a normalized database schema into robust C# entity classes and establish proper navigation properties [4, 5].
- Why explicit Fluent API relationship configurations in `OnModelCreating` are superior and safer than implicit EF Core conventions [5].
- How to configure delete behaviors (e.g., `Cascade` vs. `Restrict`) to prevent data anomalies and avoid circular cascade paths [5].
- Seeding static and reference data (such as system roles, a dummy doctor, and patient profiles) using EF Core's `HasData` method [5].
- The critical practice of reviewing generated migration files before applying them to catch schema errors early [5].

## Tasks We Completed

### Task 1: Model the Capstone Entities with Navigation Properties

We mapped our ERD entities into clean C# classes, ensuring correct database column definitions and navigation properties to represent relationships [4, 5].

```csharp
namespace CardiacMonitor.Models;

public class Patient
{
    public int Id { get; set; }
    public string? UserId { get; set; } // Foreign key to AspNetUsers
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    
    // Navigation Property representing the 1-to-Many relationship
    public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
}
```

### Task 2: Configure Explicit Relationships & Delete Behaviors

Using the Fluent API in `OnModelCreating`, we explicitly configured relationships and delete behaviors to safeguard database integrity [5].

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // Explicit 1-to-Many configuration for Patient to VitalSigns with Cascade Delete
    builder.Entity<VitalSign>()
        .HasOne(v => v.Patient)
        .WithMany(p => p.VitalSigns)
        .HasForeignKey(v => v.PatientId)
        .OnDelete(DeleteBehavior.Cascade);

    // Restrict Delete on Doctor to Appointments to prevent database circular paths
    builder.Entity<Appointment>()
        .HasOne(a => a.Doctor)
        .WithMany()
        .HasForeignKey(a => a.DoctorId)
        .OnDelete(DeleteBehavior.Restrict);
}
```

### Task 3: Seed Reference and Initial Test Data

We utilized EF Core's `HasData` to seed the database with essential lookup tables and test accounts, ensuring the API is testable immediately after migration [5].

```csharp
// Seeding system roles
builder.Entity<IdentityRole>().HasData(
    new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
    new IdentityRole { Id = doctorRoleId, Name = "Doctor", NormalizedName = "DOCTOR" },
    new IdentityRole { Id = patientRoleId, Name = "Patient", NormalizedName = "PATIENT" }
);

// Seeding test patients
builder.Entity<Patient>().HasData(
    new Patient { Id = 1, FirstName = "Ahmad", LastName = "Amr", DateOfBirth = new DateTime(1990, 5, 12), Gender = "Male", ContactNumber = "+9759835279", UserId = null }
);
```

### Task 4: Generate and Apply Code-First Migrations

We generated our migration, verified its correctness, and applied the changes to materialize the tables on our SQL Server [5].

```bash
dotnet ef migrations add InitialSchema -o Data/Migrations
dotnet ef database update
```

## Files Related to Day 2

- `Models/Patient.cs`
- `Models/VitalSign.cs`
- `Models/Medication.cs`
- `Models/Appointment.cs`
- `Data/AppDbContext.cs`
- `Data/Migrations/..._InitialSchema.cs`

## Day Result

The complete, normalized database schema was successfully established on SQL Server [5]. All core entities, relational behaviors, and initial seed data are physically materialized, providing a stable, production-grade persistence layer for the upcoming sprint features [5].