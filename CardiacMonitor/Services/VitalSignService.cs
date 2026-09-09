using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public class VitalSignService : IVitalSignService
{
    private readonly AppDbContext _context;

    // Stores the database context used by vital-sign and alert operations.
    public VitalSignService(AppDbContext context)
    {
        _context = context;
    }
     
    // Returns a filtered, sorted, and paginated vital-sign history.
    public async Task<PagedResult<VitalSignResponse>> GetVitalSignsByPatientIdAsync(
        int patientId,
        VitalSignQueryParameters queryParameters)
    {
        var query = _context.VitalSigns
            .AsNoTracking()
            .Where(vital => vital.PatientId == patientId);

        if (queryParameters.From.HasValue)
        {
            query = query.Where(vital => vital.RecordedAt >= queryParameters.From.Value);
        }

        if (queryParameters.To.HasValue)
        {
            query = query.Where(vital => vital.RecordedAt <= queryParameters.To.Value);
        }

        if (queryParameters.MinHeartRate.HasValue)
        {
            query = query.Where(vital => vital.HeartRate >= queryParameters.MinHeartRate.Value);
        }

        if (queryParameters.MaxHeartRate.HasValue)
        {
            query = query.Where(vital => vital.HeartRate <= queryParameters.MaxHeartRate.Value);
        }

        var totalCount = await query.CountAsync();
        var orderedQuery = ApplySorting(query, queryParameters.Sort);
        var items = await orderedQuery
            .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize)
            .Select(vital => new VitalSignResponse(
                vital.Id,
                vital.PatientId,
                vital.HeartRate,
                vital.OxygenSaturation,
                vital.SystolicBP,
                vital.DiastolicBP,
                vital.RecordedAt))
            .ToListAsync();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)queryParameters.PageSize);

        return new PagedResult<VitalSignResponse>(
            items,
            queryParameters.Page,
            queryParameters.PageSize,
            totalCount,
            totalPages);
    }

    // Returns one vital-sign reading by its identifier.
    public async Task<VitalSignResponse?> GetVitalSignByIdAsync(int id)
    {
        var vital = await _context.VitalSigns
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vital == null) return null;

        return new VitalSignResponse(vital.Id, vital.PatientId, vital.HeartRate, vital.OxygenSaturation, vital.SystolicBP, vital.DiastolicBP, vital.RecordedAt);
    }

    // Creates a vital-sign reading and opens an alert when thresholds are exceeded.
    public async Task<VitalSignResponse?> CreateVitalSignAsync(int patientId, CreateVitalSignRequest request)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
        if (!patientExists) return null;

        var vital = new VitalSign
        {
            PatientId = patientId,
            HeartRate = request.HeartRate,
            OxygenSaturation = request.OxygenSaturation,
            SystolicBP = request.SystolicBP,
            DiastolicBP = request.DiastolicBP,
            RecordedAt = DateTime.UtcNow 
        };

        _context.VitalSigns.Add(vital);
        SynchronizeMedicalAlert(vital);
        await _context.SaveChangesAsync();

        return new VitalSignResponse(vital.Id, vital.PatientId, vital.HeartRate, vital.OxygenSaturation, vital.SystolicBP, vital.DiastolicBP, vital.RecordedAt);
    }

    // Updates a reading and synchronizes its generated medical alert.
    public async Task<bool> UpdateVitalSignAsync(int id, UpdateVitalSignRequest request)
    {
        var vital = await _context.VitalSigns
            .Include(item => item.MedicalAlert)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (vital == null) return false;

        vital.HeartRate = request.HeartRate;
        vital.OxygenSaturation = request.OxygenSaturation;
        vital.SystolicBP = request.SystolicBP;
        vital.DiastolicBP = request.DiastolicBP;
        SynchronizeMedicalAlert(vital);

        await _context.SaveChangesAsync();
        return true;
    }

    // Deletes one vital-sign reading while preserving any linked alert audit record.
    public async Task<bool> DeleteVitalSignAsync(int id)
    {
        var vital = await _context.VitalSigns.FirstOrDefaultAsync(v => v.Id == id);
        if (vital == null) return false;

        _context.VitalSigns.Remove(vital);
        await _context.SaveChangesAsync();
        return true;
    }

    // Applies a supported deterministic sort order to a vital-sign query.
    private static IOrderedQueryable<VitalSign> ApplySorting(
        IQueryable<VitalSign> query,
        string sort)
    {
        return sort.ToLowerInvariant() switch
        {
            "recordedat_asc" => query
                .OrderBy(vital => vital.RecordedAt)
                .ThenBy(vital => vital.Id),
            "heartrate_asc" => query
                .OrderBy(vital => vital.HeartRate)
                .ThenBy(vital => vital.Id),
            "heartrate_desc" => query
                .OrderByDescending(vital => vital.HeartRate)
                .ThenBy(vital => vital.Id),
            _ => query
                .OrderByDescending(vital => vital.RecordedAt)
                .ThenByDescending(vital => vital.Id)
        };
    }

    // Creates, reopens, updates, or automatically resolves the alert for a reading.
    private void SynchronizeMedicalAlert(VitalSign vital)
    {
        var evaluation = EvaluateVitalSign(vital);
        var alert = vital.MedicalAlert;

        if (evaluation == null)
        {
            if (alert != null && alert.Status != "Resolved")
            {
                alert.Status = "Resolved";
                alert.ResolvedByUserId = null;
                alert.ResolvedAt = DateTime.UtcNow;
            }

            return;
        }

        if (alert == null)
        {
            alert = new MedicalAlert
            {
                PatientId = vital.PatientId,
                VitalSign = vital,
                CreatedAt = DateTime.UtcNow
            };
            vital.MedicalAlert = alert;
            _context.MedicalAlerts.Add(alert);
        }

        alert.Severity = evaluation.Value.Severity;
        alert.Message = evaluation.Value.Message;
        alert.Status = "Open";
        alert.AcknowledgedByUserId = null;
        alert.AcknowledgedAt = null;
        alert.ResolvedByUserId = null;
        alert.ResolvedAt = null;
    }

    // Evaluates demonstration thresholds and returns the highest alert severity.
    private static (string Severity, string Message)? EvaluateVitalSign(VitalSign vital)
    {
        var severity = GetHighestSeverity(vital);
        if (severity == null)
        {
            return null;
        }

        var abnormalValues = new List<string>();
        if (vital.HeartRate is < 60 or > 100)
        {
            abnormalValues.Add($"heart rate {vital.HeartRate} bpm");
        }

        if (vital.OxygenSaturation < 95)
        {
            abnormalValues.Add($"oxygen saturation {vital.OxygenSaturation}%");
        }

        if (vital.SystolicBP is < 100 or > 140)
        {
            abnormalValues.Add($"systolic pressure {vital.SystolicBP} mmHg");
        }

        if (vital.DiastolicBP is < 65 or > 90)
        {
            abnormalValues.Add($"diastolic pressure {vital.DiastolicBP} mmHg");
        }

        return (
            severity,
            $"{severity} demonstration threshold detected: {string.Join(", ", abnormalValues)}.");
    }

    // Selects the most severe threshold exceeded by the reading.
    private static string? GetHighestSeverity(VitalSign vital)
    {
        if (vital.HeartRate is < 40 or > 180 ||
            vital.OxygenSaturation < 85 ||
            vital.SystolicBP is < 80 or > 200 ||
            vital.DiastolicBP is < 50 or > 120)
        {
            return "Critical";
        }

        if (vital.HeartRate is < 50 or > 140 ||
            vital.OxygenSaturation < 90 ||
            vital.SystolicBP is < 90 or > 180 ||
            vital.DiastolicBP is < 60 or > 110)
        {
            return "High";
        }

        if (vital.HeartRate is < 60 or > 100 ||
            vital.OxygenSaturation < 95 ||
            vital.SystolicBP is < 100 or > 140 ||
            vital.DiastolicBP is < 65 or > 90)
        {
            return "Medium";
        }

        return null;
    }
}
