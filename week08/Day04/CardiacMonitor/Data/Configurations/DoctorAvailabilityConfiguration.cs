using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class DoctorAvailabilityConfiguration
    : IEntityTypeConfiguration<DoctorAvailability>
{
    // Configures Doctor schedule ranges, uniqueness, and integrity rules.
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.Property(slot => slot.StartTime).HasColumnType("time");
        builder.Property(slot => slot.EndTime).HasColumnType("time");

        builder.HasIndex(slot => new
        {
            slot.DoctorProfileId,
            slot.DayOfWeek,
            slot.StartTime,
            slot.EndTime
        }).IsUnique();
        builder.HasIndex(slot => new
        {
            slot.DoctorProfileId,
            slot.DayOfWeek,
            slot.IsActive
        });

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_DoctorAvailability_DayOfWeek",
                "[DayOfWeek] BETWEEN 0 AND 6");
            table.HasCheckConstraint(
                "CK_DoctorAvailability_TimeRange",
                "[StartTime] < [EndTime]");
        });

        builder.HasOne(slot => slot.DoctorProfile)
            .WithMany(doctor => doctor.AvailabilitySlots)
            .HasForeignKey(slot => slot.DoctorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
