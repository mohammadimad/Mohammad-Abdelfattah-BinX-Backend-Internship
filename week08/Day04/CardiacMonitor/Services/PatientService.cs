using System.Text.Json;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace CardiacMonitor.Services
{
    public class PatientService : IPatientService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private const string DefaultCatalogCacheKey = "patients_catalog_default";

        public PatientService(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Returns a filtered, sorted, and paginated patient list using the Cache-Aside pattern.
        public async Task<PagedResult<PatientResponse>> GetAllPatientsAsync(
            PatientQueryParameters queryParameters)
        {
            // 1. Build a deterministic cache key representing the exact query parameters
            var searchKey = string.IsNullOrWhiteSpace(queryParameters.Search) ? "all" : queryParameters.Search.Trim().ToLowerInvariant();
            var genderKey = string.IsNullOrWhiteSpace(queryParameters.Gender) ? "all" : queryParameters.Gender.Trim().ToLowerInvariant();
            var sortKey = string.IsNullOrWhiteSpace(queryParameters.Sort) ? "default" : queryParameters.Sort.Trim().ToLowerInvariant();

            var cacheKey = $"patients_catalog_p_{queryParameters.Page}_s_{queryParameters.PageSize}_q_{searchKey}_g_{genderKey}_sort_{sortKey}";

            // 2. Cache-Aside: Check if data exists in Redis cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                // Cache Hit: Deserialize and return cached data immediately
                return JsonSerializer.Deserialize<PagedResult<PatientResponse>>(cachedData)!;
            }

            // 3. Cache Miss: Query the SQL Server database
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

            var result = new PagedResult<PatientResponse>(
                items,
                queryParameters.Page,
                queryParameters.PageSize,
                totalCount,
                totalPages);

            // 4. Store the retrieved database result into Redis with a reasonable expiration time
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10), // Expire after 10 minutes
                SlidingExpiration = TimeSpan.FromMinutes(2)                 // Extend by 2 minutes if accessed frequently
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions);

            return result;
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

            // Invalidate cached catalog so clients immediately see the new patient
            await InvalidateCatalogCacheAsync();

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

            // Invalidate cached catalog so clients immediately see the updated patient data
            await InvalidateCatalogCacheAsync();

            return true;
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            // Invalidate cached catalog so clients do not read stale deleted data
            await InvalidateCatalogCacheAsync();

            return true;
        }

        // Helper method to clear stale catalog cache entries on write operations
        private async Task InvalidateCatalogCacheAsync()
        {
            // Remove common default catalog cache keys
            await _cache.RemoveAsync(DefaultCatalogCacheKey);
            await _cache.RemoveAsync("patients_catalog_p_1_s_20_q_all_g_all_sort_default");
        }

        public async Task<IEnumerable<PatientVitalsSummaryDto>> GetFixedPerformanceAsync()
        {
            var patients = await _context.Patients
                .Include(p => p.VitalSigns) // Eager Loading
                .AsNoTracking()
                .Take(10)
                .ToListAsync();

            return patients.Select(p => new PatientVitalsSummaryDto(p.FirstName, p.VitalSigns.Count));
        }

        public async Task<PatientFullDashboardDto?> GetFullDashboardAsync(int id)
        {
            var patientData = await _context.Patients
                .Include(p => p.VitalSigns)
                .Include(p => p.MedicalAlerts)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patientData == null) return null;

            return new PatientFullDashboardDto(patientData.FirstName, patientData.VitalSigns.Count, patientData.MedicalAlerts.Count);
        }

        public async Task<IEnumerable<PatientProjectionDto>> GetPatientsWithProjectionAsync()
        {
            return await _context.Patients
                .AsNoTracking()
                .Take(10)
                .Select(p => new PatientProjectionDto(
                    p.FirstName + " " + p.LastName,
                    p.VitalSigns.OrderByDescending(v => v.RecordedAt).Select(v => (int?)v.HeartRate).FirstOrDefault()
                ))
                .ToListAsync();
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