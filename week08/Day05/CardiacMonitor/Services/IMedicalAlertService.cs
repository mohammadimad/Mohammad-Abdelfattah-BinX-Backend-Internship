using CardiacMonitor.DTOs;

namespace CardiacMonitor.Services;

public interface IMedicalAlertService
{
    // Returns a patient's alerts with newest and unresolved items first.
    Task<IReadOnlyList<MedicalAlertResponse>> GetPatientAlertsAsync(
        int patientId);

    // Returns one medical alert by its identifier.
    Task<MedicalAlertResponse?> GetByIdAsync(int id);

    // Records which authenticated staff member acknowledged an open alert.
    Task<MedicalAlertActionResult> AcknowledgeAsync(
        int id,
        string userId);

    // Resolves an open or acknowledged alert and records the responsible user.
    Task<MedicalAlertActionResult> ResolveAsync(
        int id,
        string userId);
}
