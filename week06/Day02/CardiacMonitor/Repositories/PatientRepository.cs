using CardiacMonitor.Data;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Repositories;

// This is the actual implementation that interacts with the database, while the test replaces it with a mockup..
public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;

    public PatientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Patient>> GetAllAsync()
    {
        return await _context.Patients.AsNoTracking().ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(int id, bool trackChanges = false)
    {
        IQueryable<Patient> query = _context.Patients;
        if (!trackChanges) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(patient => patient.Id == id);
    }

    public async Task AddAsync(Patient patient) => await _context.Patients.AddAsync(patient);
    public void Remove(Patient patient) => _context.Patients.Remove(patient);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
