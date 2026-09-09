using Microsoft.AspNetCore.Identity;

namespace CardiacMonitor.Models;

public class DoctorProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public IdentityUser User { get; set; } = null!;
    public ICollection<DoctorAvailability> AvailabilitySlots { get; set; } =
        new List<DoctorAvailability>();
}
