using Microsoft.AspNetCore.Identity;

namespace CardiacMonitor.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public string DoctorId { get; set; } = string.Empty; 
        public IdentityUser Doctor { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
        public string? Notes { get; set; }
    }
}
