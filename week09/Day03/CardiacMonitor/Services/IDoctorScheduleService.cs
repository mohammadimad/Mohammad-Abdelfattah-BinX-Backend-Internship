using System.Security.Claims;
using CardiacMonitor.DTOs;

namespace CardiacMonitor.Services;

public interface IDoctorScheduleService
{
    // Creates or reactivates a non-overlapping Doctor availability slot.
    Task<DoctorAvailabilityResult> AddAvailabilityAsync(
        int doctorProfileId,
        CreateDoctorAvailabilityRequest request);

    // Returns the weekly availability configured for a Doctor profile.
    Task<IReadOnlyList<DoctorAvailabilityResponse>> GetAvailabilityAsync(
        int doctorProfileId);

    // Returns one availability slot by its identifier.
    Task<DoctorAvailabilityResponse?> GetAvailabilityByIdAsync(int id);

    // Deactivates an availability slot without deleting its history.
    Task<bool> DeactivateAvailabilityAsync(int id);

    // Checks whether the current user may manage a Doctor profile schedule.
    Task<bool> CanManageDoctorAsync(
        ClaimsPrincipal user,
        int doctorProfileId);

    // Checks whether a Doctor identity is working at a requested UTC date and time.
    Task<bool> IsDoctorAvailableAsync(
        string doctorUserId,
        DateTime appointmentDate);
}
