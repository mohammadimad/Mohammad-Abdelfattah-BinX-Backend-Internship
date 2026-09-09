using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class PatientCareAssignmentConfiguration
    : IEntityTypeConfiguration<PatientCareAssignment>
{
    // Configures care-assignment relationships, constraints, and lookup indexes.
    public void Configure(EntityTypeBuilder<PatientCareAssignment> builder)
    {
        builder.Property(assignment => assignment.AssignedByUserId)
            .IsRequired()
            .HasMaxLength(450);
        builder.Property(assignment => assignment.Notes)
            .HasMaxLength(500);

        builder.HasIndex(assignment => new
        {
            assignment.PatientId,
            assignment.NurseProfileId
        }).IsUnique();
        builder.HasIndex(assignment => new
        {
            assignment.NurseProfileId,
            assignment.IsActive
        });
        builder.HasIndex(assignment => new
        {
            assignment.PatientId,
            assignment.IsActive
        });

        builder.ToTable(table => table.HasCheckConstraint(
            "CK_PatientCareAssignments_EndState",
            "([IsActive] = 1 AND [EndedAt] IS NULL) OR " +
            "([IsActive] = 0 AND [EndedAt] IS NOT NULL)"));

        builder.HasOne(assignment => assignment.Patient)
            .WithMany(patient => patient.CareAssignments)
            .HasForeignKey(assignment => assignment.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.NurseProfile)
            .WithMany(nurse => nurse.CareAssignments)
            .HasForeignKey(assignment => assignment.NurseProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(assignment => assignment.AssignedByUser)
            .WithMany()
            .HasForeignKey(assignment => assignment.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
