using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public class MedicationService : IMedicationService
{
    private readonly AppDbContext _context;

    public MedicationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MedicationResponse>> GetMedicationsByPatientIdAsync(int patientId)
    {
        return await _context.Medications
            .AsNoTracking()
            .Where(m => m.PatientId == patientId)
            .Select(m => new MedicationResponse(m.Id, m.PatientId, m.Name, m.Dosage, m.Frequency, m.StartDate, m.EndDate, m.IsActive))
            .ToListAsync();
    }

    public async Task<MedicationResponse?> GetMedicationByIdAsync(int id)
    {
        var med = await _context.Medications
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (med == null) return null;

        return new MedicationResponse(med.Id, med.PatientId, med.Name, med.Dosage, med.Frequency, med.StartDate, med.EndDate, med.IsActive);
    }

    public async Task<MedicationResponse?> CreateMedicationAsync(int patientId, CreateMedicationRequest request)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == patientId);
        if (!patientExists) return null; 

        var med = new Medication
        {
            PatientId = patientId,
            Name = request.Name,
            Dosage = request.Dosage,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };

        _context.Medications.Add(med);
        await _context.SaveChangesAsync();

        return new MedicationResponse(med.Id, med.PatientId, med.Name, med.Dosage, med.Frequency, med.StartDate, med.EndDate, med.IsActive);
    }

    public async Task<bool> UpdateMedicationAsync(int id, UpdateMedicationRequest request)
    {
        var med = await _context.Medications.FirstOrDefaultAsync(m => m.Id == id);
        if (med == null) return false;

        med.Name = request.Name;
        med.Dosage = request.Dosage;
        med.Frequency = request.Frequency;
        med.StartDate = request.StartDate;
        med.EndDate = request.EndDate;
        med.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMedicationAsync(int id)
    {
        var med = await _context.Medications.FirstOrDefaultAsync(m => m.Id == id);
        if (med == null) return false;

        _context.Medications.Remove(med);
        await _context.SaveChangesAsync();
        return true;
    }
}