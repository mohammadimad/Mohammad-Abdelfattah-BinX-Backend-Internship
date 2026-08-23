using CardiacMonitor.Data;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Repositories;

// Uses EF Core in production while unit tests replace this dependency with a mock.
public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;

    // Stores the EF Core context used by repository methods.
    public PatientRepository(AppDbContext context)
    {
        _context = context;
    }

    // Returns all patients without EF Core change tracking.
    public async Task<IReadOnlyList<Patient>> GetAllAsync()
    {
        return await _context.Patients.AsNoTracking().ToListAsync();
    }

    // Returns one patient and enables tracking only when an update needs it.
    public async Task<Patient?> GetByIdAsync(int id, bool trackChanges = false)
    {
        IQueryable<Patient> query = _context.Patients;
        if (!trackChanges) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(patient => patient.Id == id);
    }

    // Adds a patient to the EF Core context.
    public async Task AddAsync(Patient patient) => await _context.Patients.AddAsync(patient);

    // Marks a patient entity for deletion.
    public void Remove(Patient patient) => _context.Patients.Remove(patient);

    // Saves all pending changes to the database.
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
