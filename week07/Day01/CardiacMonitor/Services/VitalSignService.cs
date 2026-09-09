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
     
     public async Task<IEnumerable<VitalSignResponse>> GetVitalSignsByPatientIdAsync(int patientId)
    {
        return await _context.VitalSigns
            .AsNoTracking()
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.RecordedAt)
            .Select(v => new VitalSignResponse(v.Id, v.PatientId, v.HeartRate, v.OxygenSaturation, v.SystolicBP, v.DiastolicBP, v.RecordedAt))
            .ToListAsync();
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
}