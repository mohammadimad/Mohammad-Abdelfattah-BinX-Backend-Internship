using CardiacMonitor.Models;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services
{
    public class PatientService : IPatientService
    {
        private readonly AppDbContext _context;

        public PatientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PatientResponse>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .AsNoTracking()

        .Select(p => new PatientResponse(p.Id, p.UserId, p.FirstName, p.LastName, p.DateOfBirth, p.Gender, p.ContactNumber))
                .ToListAsync();
        }

        public async Task<PatientResponse?> GetPatientByIdAsync(int id)
        {
            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

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

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return new PatientResponse(patient.Id, patient.UserId, patient.FirstName, patient.LastName, patient.DateOfBirth, patient.Gender, patient.ContactNumber);
        }

        public async Task<bool> UpdatePatientAsync(int id, UpdatePatientRequest request)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return false;

            patient.FirstName = request.FirstName;
            patient.LastName = request.LastName;
            patient.DateOfBirth = request.DateOfBirth;
            patient.Gender = request.Gender;
            patient.ContactNumber = request.ContactNumber;

            await _context.SaveChangesAsync(); 
            return true;
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync(); 
            return true;
        }
    }
}