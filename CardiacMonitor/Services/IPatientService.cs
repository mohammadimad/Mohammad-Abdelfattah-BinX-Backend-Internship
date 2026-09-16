using  CardiacMonitor.DTOs;

namespace CardiacMonitor.Services
{
   
        public interface IPatientService
        {
            // Returns a filtered, sorted, and paginated patient list.
            Task<PagedResult<PatientResponse>> GetAllPatientsAsync(PatientQueryParameters queryParameters);
            Task<PatientResponse?> GetPatientByIdAsync(int id);
            // Returns one patient with related clinical collections through split queries.
            Task<PatientClinicalDetailsResponse?> GetClinicalDetailsAsync(int id);
            Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request);
            Task<bool> UpdatePatientAsync(int id, UpdatePatientRequest request);
            Task<bool> DeletePatientAsync(int id);

    }
}
