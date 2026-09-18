using Microsoft.AspNetCore.Identity;

namespace CardiacMonitor.Models;

public class NurseProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;

    public IdentityUser User { get; set; } = null!;
    public ICollection<PatientCareAssignment> CareAssignments { get; set; } =
        new List<PatientCareAssignment>();
}
