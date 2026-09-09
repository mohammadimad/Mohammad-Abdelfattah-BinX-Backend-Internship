using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public class MedicationService : IMedicationService
{
    private readonly AppDbContext _context;

    // Initializes the medication service.
    public MedicationService(AppDbContext context)
    {
        _context = context;
    }

    // Retrieves all medications for a patient.
    public async Task<IEnumerable<MedicationResponse>> GetMedicationsByPatientIdAsync(int patientId)
    {
        return await _context.Medications
            .AsNoTracking()
            .Where(m => m.PatientId == patientId)
            .Select(m => new MedicationResponse(m.Id, m.PatientId, m.Name, m.Dosage, m.Frequency, m.StartDate, m.EndDate, m.IsActive, m.StockQuantity, m.UnitPrice))
            .ToListAsync();
    }

    // Retrieves a medication by its identifier.
    public async Task<MedicationResponse?> GetMedicationByIdAsync(int id)
    {
        var med = await _context.Medications
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (med == null) return null;

        return new MedicationResponse(med.Id, med.PatientId, med.Name, med.Dosage, med.Frequency, med.StartDate, med.EndDate, med.IsActive, med.StockQuantity, med.UnitPrice);
    }

    // Creates a medication for an existing patient.
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
            IsActive = request.IsActive,
            StockQuantity = request.StockQuantity,
            UnitPrice = request.UnitPrice
        };

        _context.Medications.Add(med);
        await _context.SaveChangesAsync();

        return new MedicationResponse(med.Id, med.PatientId, med.Name, med.Dosage, med.Frequency, med.StartDate, med.EndDate, med.IsActive, med.StockQuantity, med.UnitPrice);
    }

    // Updates an existing medication.
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
        med.StockQuantity = request.StockQuantity;
        med.UnitPrice = request.UnitPrice;

        await _context.SaveChangesAsync();
        return true;
    }

    // Deletes an existing medication.
    public async Task<bool> DeleteMedicationAsync(int id)
    {
        var med = await _context.Medications.FirstOrDefaultAsync(m => m.Id == id);
        if (med == null) return false;

        _context.Medications.Remove(med);
        await _context.SaveChangesAsync();
        return true;
    }
}
