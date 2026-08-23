using CardiacMonitor.Models;

namespace CardiacMonitor.Repositories;

// Separates PatientService from EF Core so the dependency can be mocked in unit tests.
public interface IPatientRepository
{
    // Retrieves every patient without modifying stored data.
    Task<IReadOnlyList<Patient>> GetAllAsync();

    // Retrieves one patient and optionally enables change tracking.
    Task<Patient?> GetByIdAsync(int id, bool trackChanges = false);

    // Adds a new patient to the current unit of work.
    Task AddAsync(Patient patient);

    // Marks a patient for deletion.
    void Remove(Patient patient);

    // Saves the pending patient changes.
    Task SaveChangesAsync();
}
