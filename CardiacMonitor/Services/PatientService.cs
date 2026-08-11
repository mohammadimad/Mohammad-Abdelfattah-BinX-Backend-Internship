using static CardiacMonitor.DTOs.PatientDtos;

namespace CardiacMonitor.Services
{
    public class PatientService : IPatientService
    {

        private static readonly List<PatientResponse> _mockPatients = new()
    {
        new PatientResponse(1, "Ahmad", "Amr", new DateTime(1990, 5, 12), "Male", "+970599000001"),
        new PatientResponse(2, "Sara", "Ali", new DateTime(1985, 10, 22), "Female", "+970599000002")
    };

        public Task<IEnumerable<PatientResponse>> GetAllPatientsAsync()
        {
            return Task.FromResult<IEnumerable<PatientResponse>>(_mockPatients);
        }

        public Task<PatientResponse?> GetPatientByIdAsync(int id)
        {
            var patient = _mockPatients.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(patient);
        }
    }
}