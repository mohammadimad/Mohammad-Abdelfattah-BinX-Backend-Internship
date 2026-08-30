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

        // Returns a filtered, sorted, and paginated patient list.
        public async Task<PagedResult<PatientResponse>> GetAllPatientsAsync(
            PatientQueryParameters queryParameters)
        {
            var query = _context.Patients.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(queryParameters.Search))
            {
                var search = queryParameters.Search.Trim().ToLower();
                query = query.Where(patient =>
                    patient.FirstName.ToLower().Contains(search) ||
                    patient.LastName.ToLower().Contains(search) ||
                    (patient.FirstName + " " + patient.LastName)
                        .ToLower()
                        .Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.Gender))
            {
                var gender = queryParameters.Gender.Trim().ToLower();
                query = query.Where(patient => patient.Gender.ToLower() == gender);
            }

            var totalCount = await query.CountAsync();
            var orderedQuery = ApplySorting(query, queryParameters.Sort);
            var items = await orderedQuery
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .Select(patient => new PatientResponse(
                    patient.Id,
                    patient.UserId,
                    patient.FirstName,
                    patient.LastName,
                    patient.DateOfBirth,
                    patient.Gender,
                    patient.ContactNumber))
                .ToListAsync();

            var totalPages = totalCount == 0
                ? 0
                : (int)Math.Ceiling(totalCount / (double)queryParameters.PageSize);

            return new PagedResult<PatientResponse>(
                items,
                queryParameters.Page,
                queryParameters.PageSize,
                totalCount,
                totalPages);
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

        // Applies a supported deterministic sort order to a patient query.
        private static IOrderedQueryable<Patient> ApplySorting(
            IQueryable<Patient> query,
            string sort)
        {
            return sort.ToLowerInvariant() switch
            {
                "firstname_desc" => query
                    .OrderByDescending(patient => patient.FirstName)
                    .ThenByDescending(patient => patient.Id),
                "lastname_asc" => query
                    .OrderBy(patient => patient.LastName)
                    .ThenBy(patient => patient.FirstName)
                    .ThenBy(patient => patient.Id),
                "lastname_desc" => query
                    .OrderByDescending(patient => patient.LastName)
                    .ThenByDescending(patient => patient.FirstName)
                    .ThenByDescending(patient => patient.Id),
                "dateofbirth_asc" => query
                    .OrderBy(patient => patient.DateOfBirth)
                    .ThenBy(patient => patient.Id),
                "dateofbirth_desc" => query
                    .OrderByDescending(patient => patient.DateOfBirth)
                    .ThenByDescending(patient => patient.Id),
                _ => query
                    .OrderBy(patient => patient.FirstName)
                    .ThenBy(patient => patient.LastName)
                    .ThenBy(patient => patient.Id)
            };
        }
    }
}
