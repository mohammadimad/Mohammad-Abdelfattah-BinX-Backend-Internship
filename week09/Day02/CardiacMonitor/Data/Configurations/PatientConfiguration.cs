using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    // Configures patient fields and the optional one-to-one identity link.
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(patient => patient.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(patient => patient.LastName).IsRequired().HasMaxLength(100);
        builder.Property(patient => patient.Gender).IsRequired().HasMaxLength(20);
        builder.Property(patient => patient.ContactNumber).IsRequired().HasMaxLength(30);
        builder.Property(patient => patient.UserId).HasMaxLength(450);

        builder.HasIndex(patient => patient.UserId).IsUnique();

        builder.HasOne<IdentityUser>()
            .WithOne()
            .HasForeignKey<Patient>(patient => patient.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
