using CardiacMonitor.DTOs;

namespace CardiacMonitor.Services;

public interface IStaffService
{
    // Creates a Nurse identity and linked professional profile atomically.
    Task<NurseCreationResult> CreateNurseAsync(CreateNurseRequest request);

    // Returns the nurse profiles available for care assignments.
    Task<IReadOnlyList<NurseResponse>> GetNursesAsync();

    // Creates a Doctor identity and linked professional profile atomically.
    Task<DoctorCreationResult> CreateDoctorAsync(CreateDoctorRequest request);

    // Returns active and inactive Doctor professional profiles.
    Task<IReadOnlyList<DoctorResponse>> GetDoctorsAsync();
}
