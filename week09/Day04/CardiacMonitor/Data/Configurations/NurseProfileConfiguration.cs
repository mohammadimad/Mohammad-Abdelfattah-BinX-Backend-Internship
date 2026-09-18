using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class NurseProfileConfiguration
    : IEntityTypeConfiguration<NurseProfile>
{
    // Configures nurse profile fields and its one-to-one Identity user link.
    public void Configure(EntityTypeBuilder<NurseProfile> builder)
    {
        builder.Property(nurse => nurse.UserId)
            .IsRequired()
            .HasMaxLength(450);
        builder.Property(nurse => nurse.FullName)
            .IsRequired()
            .HasMaxLength(150);
        builder.Property(nurse => nurse.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(nurse => nurse.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(nurse => nurse.UserId).IsUnique();
        builder.HasIndex(nurse => nurse.LicenseNumber).IsUnique();

        builder.HasOne(nurse => nurse.User)
            .WithOne()
            .HasForeignKey<NurseProfile>(nurse => nurse.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
