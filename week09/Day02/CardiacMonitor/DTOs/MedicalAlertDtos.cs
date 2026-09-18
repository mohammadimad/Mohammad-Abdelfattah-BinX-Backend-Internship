namespace CardiacMonitor.DTOs;

public sealed record MedicalAlertResponse(
    int Id,
    int PatientId,
    int? VitalSignId,
    string Severity,
    string Status,
    string Message,
    DateTime CreatedAt,
    string? AcknowledgedByUserId,
    DateTime? AcknowledgedAt,
    string? ResolvedByUserId,
    DateTime? ResolvedAt);

public sealed record MedicalAlertActionResult(
    bool Succeeded,
    string Message,
    MedicalAlertResponse? Alert = null);
