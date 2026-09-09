using  CardiacMonitor.DTOs;

namespace CardiacMonitor.Services
{
   
        public interface IPatientService
        {
        // Updated function for page splitting, filtering, and sorting
        Task<PaginatedList<PatientResponse>> GetPaginatedPatientsAsync(
            int pageNumber,
            int pageSize,
            string? searchName,
            string? gender,
            string? sortBy,
            bool isDescending);
             Task<PatientResponse?> GetPatientByIdAsync(int id);
            Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request);
            Task<bool> UpdatePatientAsync(int id, UpdatePatientRequest request);
            Task<bool> DeletePatientAsync(int id);

    }
}
