using System.Security.Claims;
using CardiacMonitor.Data;
using CardiacMonitor.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public sealed class PatientAccessService : IPatientAccessService
{
    private readonly AppDbContext _context;

    // Stores the database context used by resource-based access checks.
    public PatientAccessService(AppDbContext context)
    {
        _context = context;
    }

    // Checks role, ownership, or active nurse assignment for patient access.
    public async Task<bool> CanAccessPatientAsync(
        ClaimsPrincipal user,
        int patientId)
    {
        if (user.IsInRole("Admin") || user.IsInRole("Doctor"))
        {
            return true;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        if (user.IsInRole("Patient"))
        {
            var patientIdClaim = user.FindFirstValue(CustomClaimTypes.PatientId);
            if (int.TryParse(patientIdClaim, out var claimedPatientId))
            {
                return claimedPatientId == patientId;
            }

            return await _context.Patients.AnyAsync(patient =>
                patient.Id == patientId && patient.UserId == userId);
        }

        if (user.IsInRole("Nurse"))
        {
            return await _context.PatientCareAssignments.AnyAsync(assignment =>
                assignment.PatientId == patientId &&
                assignment.IsActive &&
                assignment.NurseProfile.UserId == userId);
        }

        return false;
    }
}
