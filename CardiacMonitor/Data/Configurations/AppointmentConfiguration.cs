using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    // Configures appointment fields, scheduling indexes, and relationships.
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(appointment => appointment.DoctorId).IsRequired().HasMaxLength(450);
        builder.Property(appointment => appointment.Status).IsRequired().HasMaxLength(30);
        builder.Property(appointment => appointment.Notes).HasMaxLength(1000);

        builder.HasIndex(appointment => new
            {
                appointment.DoctorId,
                appointment.AppointmentDate
            })
            .IsUnique()
            .HasDatabaseName("UX_Appointments_DoctorId_AppointmentDate");
        builder.HasIndex(appointment => new
            {
                appointment.PatientId,
                appointment.AppointmentDate
            })
            .HasDatabaseName("IX_Appointments_PatientId_AppointmentDate");

        builder.HasOne(appointment => appointment.Patient)
            .WithMany()
            .HasForeignKey(appointment => appointment.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(appointment => appointment.Doctor)
            .WithMany()
            .HasForeignKey(appointment => appointment.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "CK_Appointments_Status",
                "[Status] IN ('Scheduled', 'Completed', 'Cancelled')"));
    }
}
