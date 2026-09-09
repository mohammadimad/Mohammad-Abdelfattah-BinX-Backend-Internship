using CardiacMonitor.DTOs;

namespace CardiacMonitor.Services;

public interface ICareAssignmentService
{
    // Creates or reactivates a nurse assignment for a patient.
    Task<CareAssignmentResult> AssignNurseAsync(
        int patientId,
        CreateCareAssignmentRequest request,
        string assignedByUserId);

    // Returns every current and historical assignment for a patient.
    Task<IReadOnlyList<CareAssignmentResponse>> GetPatientAssignmentsAsync(
        int patientId);

    // Returns the active patients assigned to the current nurse identity.
    Task<IReadOnlyList<PatientResponse>> GetAssignedPatientsAsync(
        string nurseUserId);

    // Ends an active care assignment without deleting its audit history.
    Task<bool> EndAssignmentAsync(int assignmentId);

    // Checks whether a Nurse identity has an active assignment to a patient.
    Task<bool> IsNurseAssignedAsync(string nurseUserId, int patientId);
}
