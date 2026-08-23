using CardiacMonitor.Models;

namespace CardiacMonitor.Repositories;

// This contract separates the PatientService logic from the EF Core so that we can perform a mock in unit testing.
public interface IPatientRepository
{
    Task<IReadOnlyList<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id, bool trackChanges = false);
    Task AddAsync(Patient patient);
    void Remove(Patient patient);
    Task SaveChangesAsync();
}
