using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public class VitalSignService : IVitalSignService
{
    private readonly AppDbContext _context;

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

    public async Task<VitalSignResponse?> GetVitalSignByIdAsync(int id)
    {
        var vital = await _context.VitalSigns
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vital == null) return null;

        return new VitalSignResponse(vital.Id, vital.PatientId, vital.HeartRate, vital.OxygenSaturation, vital.SystolicBP, vital.DiastolicBP, vital.RecordedAt);
    }

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
        await _context.SaveChangesAsync();

        return new VitalSignResponse(vital.Id, vital.PatientId, vital.HeartRate, vital.OxygenSaturation, vital.SystolicBP, vital.DiastolicBP, vital.RecordedAt);
    }

    public async Task<bool> UpdateVitalSignAsync(int id, UpdateVitalSignRequest request)
    {
        var vital = await _context.VitalSigns.FirstOrDefaultAsync(v => v.Id == id);
        if (vital == null) return false;

        vital.HeartRate = request.HeartRate;
        vital.OxygenSaturation = request.OxygenSaturation;
        vital.SystolicBP = request.SystolicBP;
        vital.DiastolicBP = request.DiastolicBP;

        await _context.SaveChangesAsync();
        return true;
    }

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
}
