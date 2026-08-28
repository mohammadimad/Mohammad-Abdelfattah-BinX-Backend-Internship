using CardiacMonitor.Models;

namespace CardiacMonitor.Repositories;

//Separate the PatientService logic from the EF Core so that we can perform a mock in the unit test.
public interface IPatientRepository
{
    Task<IReadOnlyList<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id, bool trackChanges = false);
    Task AddAsync(Patient patient);
    void Remove(Patient patient);
    Task SaveChangesAsync();
}
