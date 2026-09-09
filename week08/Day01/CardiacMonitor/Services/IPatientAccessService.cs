using System.Security.Claims;

namespace CardiacMonitor.Services;

public interface IPatientAccessService
{
    // Checks role, ownership, or active nurse assignment for patient access.
    Task<bool> CanAccessPatientAsync(ClaimsPrincipal user, int patientId);
}
