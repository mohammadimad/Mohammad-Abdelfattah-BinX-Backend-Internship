using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public sealed class MedicalAlertService : IMedicalAlertService
{
    private readonly AppDbContext _context;

    // Stores the database context used by the medical-alert workflow.
    public MedicalAlertService(AppDbContext context)
    {
        _context = context;
    }

    // Returns a patient's alerts with newest and unresolved items first.
    public async Task<IReadOnlyList<MedicalAlertResponse>> GetPatientAlertsAsync(
        int patientId)
    {
        return await _context.MedicalAlerts
            .AsNoTracking()
            .Where(alert => alert.PatientId == patientId)
            .OrderBy(alert => alert.Status == "Resolved")
            .ThenByDescending(alert => alert.CreatedAt)
            .Select(alert => ToResponse(alert))
            .ToListAsync();
    }

    // Returns one medical alert by its identifier.
    public async Task<MedicalAlertResponse?> GetByIdAsync(int id)
    {
        return await _context.MedicalAlerts
            .AsNoTracking()
            .Where(alert => alert.Id == id)
            .Select(alert => ToResponse(alert))
            .FirstOrDefaultAsync();
    }

    // Records which authenticated staff member acknowledged an open alert.
    public async Task<MedicalAlertActionResult> AcknowledgeAsync(
        int id,
        string userId)
    {
        var alert = await _context.MedicalAlerts
            .FirstOrDefaultAsync(item => item.Id == id);
        if (alert == null)
        {
            return new MedicalAlertActionResult(false, "Medical alert was not found.");
        }

        if (alert.Status != "Open")
        {
            return new MedicalAlertActionResult(
                false,
                "Only an open alert can be acknowledged.",
                ToResponse(alert));
        }

        alert.Status = "Acknowledged";
        alert.AcknowledgedByUserId = userId;
        alert.AcknowledgedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new MedicalAlertActionResult(
            true,
            "Medical alert acknowledged successfully.",
            ToResponse(alert));
    }

    // Resolves an open or acknowledged alert and records the responsible user.
    public async Task<MedicalAlertActionResult> ResolveAsync(
        int id,
        string userId)
    {
        var alert = await _context.MedicalAlerts
            .FirstOrDefaultAsync(item => item.Id == id);
        if (alert == null)
        {
            return new MedicalAlertActionResult(false, "Medical alert was not found.");
        }

        if (alert.Status == "Resolved")
        {
            return new MedicalAlertActionResult(
                false,
                "Medical alert is already resolved.",
                ToResponse(alert));
        }

        alert.Status = "Resolved";
        alert.ResolvedByUserId = userId;
        alert.ResolvedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new MedicalAlertActionResult(
            true,
            "Medical alert resolved successfully.",
            ToResponse(alert));
    }

    // Maps a medical-alert entity to its API response.
    private static MedicalAlertResponse ToResponse(MedicalAlert alert)
    {
        return new MedicalAlertResponse(
            alert.Id,
            alert.PatientId,
            alert.VitalSignId,
            alert.Severity,
            alert.Status,
            alert.Message,
            alert.CreatedAt,
            alert.AcknowledgedByUserId,
            alert.AcknowledgedAt,
            alert.ResolvedByUserId,
            alert.ResolvedAt);
    }
}
