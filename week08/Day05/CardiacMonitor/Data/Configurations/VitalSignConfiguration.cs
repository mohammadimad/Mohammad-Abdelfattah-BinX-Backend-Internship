using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class VitalSignConfiguration : IEntityTypeConfiguration<VitalSign>
{
    // Configures vital-sign integrity rules and its optimized history index.
    public void Configure(EntityTypeBuilder<VitalSign> builder)
    {
        builder.Property(vital => vital.OxygenSaturation).HasPrecision(5, 2);

        builder.HasIndex(vital => new { vital.PatientId, vital.RecordedAt })
            .HasDatabaseName("IX_VitalSigns_PatientId_RecordedAt");

        builder.HasOne(vital => vital.Patient)
            .WithMany(patient => patient.VitalSigns)
            .HasForeignKey(vital => vital.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_VitalSigns_HeartRate",
                "[HeartRate] BETWEEN 30 AND 250");
            tableBuilder.HasCheckConstraint(
                "CK_VitalSigns_OxygenSaturation",
                "CAST([OxygenSaturation] AS REAL) BETWEEN 50 AND 100");
            tableBuilder.HasCheckConstraint(
                "CK_VitalSigns_SystolicBP",
                "[SystolicBP] BETWEEN 70 AND 220");
            tableBuilder.HasCheckConstraint(
                "CK_VitalSigns_DiastolicBP",
                "[DiastolicBP] BETWEEN 40 AND 130");
        });
    }
}
