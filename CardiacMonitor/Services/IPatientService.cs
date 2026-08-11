using static CardiacMonitor.DTOs.PatientDtos;

namespace CardiacMonitor.Services
{
   
        public interface IPatientService
        {
            Task<IEnumerable<PatientResponse>> GetAllPatientsAsync();
            Task<PatientResponse?> GetPatientByIdAsync(int id);

    }
}
