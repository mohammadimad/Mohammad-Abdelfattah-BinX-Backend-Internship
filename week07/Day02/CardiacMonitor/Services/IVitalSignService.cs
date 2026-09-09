using  CardiacMonitor.DTOs;

namespace CardiacMonitor.Services
{
    public interface IVitalSignService
    {
        Task<IEnumerable<VitalSignResponse>> GetVitalSignsByPatientIdAsync(int patientId);
        Task<VitalSignResponse?> GetVitalSignByIdAsync(int id);
        Task<VitalSignResponse?> CreateVitalSignAsync(int patientId, CreateVitalSignRequest request);
        Task<bool> UpdateVitalSignAsync(int id, UpdateVitalSignRequest request);
        Task<bool> DeleteVitalSignAsync(int id);
    }
}
