using CardiacMonitor.Models;

namespace CardiacMonitor.Repositories;

// هذا العقد يفصل منطق PatientService عن EF Core حتى نستطيع عمل Mock في اختبار الوحدة.
public interface IPatientRepository
{
    Task<IReadOnlyList<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id, bool trackChanges = false);
    Task AddAsync(Patient patient);
    void Remove(Patient patient);
    Task SaveChangesAsync();
}
