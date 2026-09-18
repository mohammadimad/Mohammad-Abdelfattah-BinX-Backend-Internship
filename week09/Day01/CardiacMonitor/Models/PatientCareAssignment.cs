using Microsoft.AspNetCore.Identity;

namespace CardiacMonitor.Models;

public class PatientCareAssignment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int NurseProfileId { get; set; }
    public string AssignedByUserId { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    public Patient Patient { get; set; } = null!;
    public NurseProfile NurseProfile { get; set; } = null!;
    public IdentityUser AssignedByUser { get; set; } = null!;
}
