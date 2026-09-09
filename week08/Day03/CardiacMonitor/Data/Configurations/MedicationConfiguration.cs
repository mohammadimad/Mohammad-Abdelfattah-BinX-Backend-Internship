using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    // Configures medication fields, lookup index, and patient relationship.
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.Property(medication => medication.Name).IsRequired().HasMaxLength(150);
        builder.Property(medication => medication.Dosage).IsRequired().HasMaxLength(100);
        builder.Property(medication => medication.Frequency).IsRequired().HasMaxLength(100);

        builder.HasIndex(medication => new { medication.PatientId, medication.IsActive })
            .HasDatabaseName("IX_Medications_PatientId_IsActive");

        builder.HasOne(medication => medication.Patient)
            .WithMany()
            .HasForeignKey(medication => medication.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
