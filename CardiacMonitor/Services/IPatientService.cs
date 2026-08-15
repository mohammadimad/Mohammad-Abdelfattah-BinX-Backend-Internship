using  CardiacMonitor.DTOs;

namespace CardiacMonitor.Services
{
   
        public interface IPatientService
        {
            Task<IEnumerable<PatientResponse>> GetAllPatientsAsync();
            Task<PatientResponse?> GetPatientByIdAsync(int id);
            Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request);
            Task<bool> UpdatePatientAsync(int id, UpdatePatientRequest request);
            Task<bool> DeletePatientAsync(int id);

    }
}
