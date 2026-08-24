using CardiacMonitor.Models;
using CardiacMonitor.Repositories;
using CardiacMonitor.DTOs;

namespace CardiacMonitor.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repository;

        public PatientService(IPatientRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PatientResponse>> GetAllPatientsAsync()
        {
            var patients = await _repository.GetAllAsync();

            return patients.Select(patient => new PatientResponse(patient.Id, patient.UserId, patient.FirstName, patient.LastName, patient.DateOfBirth, patient.Gender, patient.ContactNumber));
        }

        public async Task<PatientResponse?> GetPatientByIdAsync(int id)
        {
            var patient = await _repository.GetByIdAsync(id);

            if (patient == null) return null;
            return new PatientResponse(patient.Id, patient.UserId, patient.FirstName, patient.LastName, patient.DateOfBirth, patient.Gender, patient.ContactNumber);
        }

        public async Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request)
        {
            var patient = new Patient
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                ContactNumber = request.ContactNumber
            };

            await _repository.AddAsync(patient);
            await _repository.SaveChangesAsync();

            return new PatientResponse(patient.Id, patient.UserId, patient.FirstName, patient.LastName, patient.DateOfBirth, patient.Gender, patient.ContactNumber);
        }

        public async Task<bool> UpdatePatientAsync(int id, UpdatePatientRequest request)
        {
            var patient = await _repository.GetByIdAsync(id, trackChanges: true);
            if (patient == null) return false;

            patient.FirstName = request.FirstName;
            patient.LastName = request.LastName;
            patient.DateOfBirth = request.DateOfBirth;
            patient.Gender = request.Gender;
            patient.ContactNumber = request.ContactNumber;

            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _repository.GetByIdAsync(id, trackChanges: true);
            if (patient == null) return false;

            _repository.Remove(patient);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
