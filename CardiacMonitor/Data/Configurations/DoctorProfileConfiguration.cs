using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class DoctorProfileConfiguration
    : IEntityTypeConfiguration<DoctorProfile>
{
    // Configures professional Doctor fields and the one-to-one Identity link.
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        builder.Property(doctor => doctor.UserId)
            .IsRequired()
            .HasMaxLength(450);
        builder.Property(doctor => doctor.FullName)
            .IsRequired()
            .HasMaxLength(150);
        builder.Property(doctor => doctor.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(doctor => doctor.Specialty)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(doctor => doctor.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(doctor => doctor.UserId).IsUnique();
        builder.HasIndex(doctor => doctor.LicenseNumber).IsUnique();
        builder.HasIndex(doctor => new { doctor.Specialty, doctor.IsActive });

        builder.HasOne(doctor => doctor.User)
            .WithOne()
            .HasForeignKey<DoctorProfile>(doctor => doctor.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
