using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class MedicalAlertConfiguration
    : IEntityTypeConfiguration<MedicalAlert>
{
    // Configures alert workflow constraints, audit users, and lookup indexes.
    public void Configure(EntityTypeBuilder<MedicalAlert> builder)
    {
        builder.Property(alert => alert.Severity)
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(alert => alert.Status)
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(alert => alert.Message)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(alert => alert.AcknowledgedByUserId)
            .HasMaxLength(450);
        builder.Property(alert => alert.ResolvedByUserId)
            .HasMaxLength(450);

        builder.HasIndex(alert => alert.VitalSignId)
            .IsUnique()
            .HasFilter("[VitalSignId] IS NOT NULL");
        builder.HasIndex(alert => new
        {
            alert.PatientId,
            alert.Status,
            alert.CreatedAt
        });
        builder.HasIndex(alert => new { alert.Status, alert.Severity });

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_MedicalAlerts_Severity",
                "[Severity] IN ('Medium', 'High', 'Critical')");
            table.HasCheckConstraint(
                "CK_MedicalAlerts_Status",
                "[Status] IN ('Open', 'Acknowledged', 'Resolved')");
        });

        builder.HasOne(alert => alert.Patient)
            .WithMany(patient => patient.MedicalAlerts)
            .HasForeignKey(alert => alert.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(alert => alert.VitalSign)
            .WithOne(vital => vital.MedicalAlert)
            .HasForeignKey<MedicalAlert>(alert => alert.VitalSignId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(alert => alert.AcknowledgedByUser)
            .WithMany()
            .HasForeignKey(alert => alert.AcknowledgedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(alert => alert.ResolvedByUser)
            .WithMany()
            .HasForeignKey(alert => alert.ResolvedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
