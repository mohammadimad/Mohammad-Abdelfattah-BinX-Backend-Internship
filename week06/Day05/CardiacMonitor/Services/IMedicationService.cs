using  CardiacMonitor.DTOs;

namespace CardiacMonitor.Services
{
    public interface IMedicationService
    {
        Task<IEnumerable<MedicationResponse>> GetMedicationsByPatientIdAsync(int patientId);
        Task<MedicationResponse?> GetMedicationByIdAsync(int id);
        Task<MedicationResponse?> CreateMedicationAsync(int patientId, CreateMedicationRequest request);
        Task<bool> UpdateMedicationAsync(int id, UpdateMedicationRequest request);
        Task<bool> DeleteMedicationAsync(int id);
    }
}
