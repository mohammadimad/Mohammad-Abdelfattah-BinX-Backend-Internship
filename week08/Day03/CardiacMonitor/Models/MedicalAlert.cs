using Microsoft.AspNetCore.Identity;

namespace CardiacMonitor.Models;

public class MedicalAlert
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? VitalSignId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AcknowledgedByUserId { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Patient Patient { get; set; } = null!;
    public VitalSign? VitalSign { get; set; }
    public IdentityUser? AcknowledgedByUser { get; set; }
    public IdentityUser? ResolvedByUser { get; set; }
}
