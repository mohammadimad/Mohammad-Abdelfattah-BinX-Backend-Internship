namespace CardiacMonitor.DTOs;

public sealed record CreateNurseRequest(
    string Email,
    string Password,
    string FullName,
    string LicenseNumber,
    string Department);

public sealed record NurseResponse(
    int Id,
    string UserId,
    string Email,
    string FullName,
    string LicenseNumber,
    string Department);

public sealed record NurseCreationResult(
    bool Succeeded,
    string Message,
    NurseResponse? Nurse = null);

public sealed record CreateCareAssignmentRequest(
    int NurseProfileId,
    string? Notes);

public sealed record CareAssignmentResponse(
    int Id,
    int PatientId,
    int NurseProfileId,
    string NurseName,
    string AssignedByUserId,
    DateTime AssignedAt,
    DateTime? EndedAt,
    bool IsActive,
    string? Notes);

public sealed record CareAssignmentResult(
    bool Succeeded,
    string Message,
    CareAssignmentResponse? Assignment = null);
