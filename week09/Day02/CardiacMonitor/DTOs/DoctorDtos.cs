namespace CardiacMonitor.DTOs;

public sealed record CreateDoctorRequest(
    string Email,
    string Password,
    string FullName,
    string LicenseNumber,
    string Specialty,
    string Department);

public sealed record DoctorResponse(
    int Id,
    string UserId,
    string Email,
    string FullName,
    string LicenseNumber,
    string Specialty,
    string Department,
    bool IsActive);

public sealed record DoctorCreationResult(
    bool Succeeded,
    string Message,
    DoctorResponse? Doctor = null);

public sealed record CreateDoctorAvailabilityRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

public sealed record DoctorAvailabilityResponse(
    int Id,
    int DoctorProfileId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public sealed record DoctorAvailabilityResult(
    bool Succeeded,
    string Message,
    DoctorAvailabilityResponse? Availability = null);
