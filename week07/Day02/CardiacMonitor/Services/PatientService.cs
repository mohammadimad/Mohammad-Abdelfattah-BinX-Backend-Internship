using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _context;

    // العودة للحقن المباشر والسليم للـ DbContext
    public PatientService(AppDbContext context)
    {
        _context = context;
    }

    // 1. جلب المرضى مقسمين لصفحات مع الفلترة والترتيب
    public async Task<PaginatedList<PatientResponse>> GetPaginatedPatientsAsync(
        int pageNumber,
        int pageSize,
        string? searchName,
        string? gender,
        string? sortBy,
        bool isDescending)
    {
        var query = _context.Patients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            query = query.Where(p => p.FirstName.Contains(searchName) || p.LastName.Contains(searchName));
        }

        if (!string.IsNullOrWhiteSpace(gender))
        {
            query = query.Where(p => p.Gender == gender);
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortBy.ToLower() switch
            {
                "lastname" => isDescending ? query.OrderByDescending(p => p.LastName) : query.OrderBy(p => p.LastName),
                "dateofbirth" => isDescending ? query.OrderByDescending(p => p.DateOfBirth) : query.OrderBy(p => p.DateOfBirth),
                _ => isDescending ? query.OrderByDescending(p => p.FirstName) : query.OrderBy(p => p.FirstName)
            };
        }
        else
        {
            query = query.OrderBy(p => p.FirstName);
        }

        var totalCount = await query.CountAsync();

        var patients = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientResponse(p.Id, p.UserId, p.FirstName, p.LastName, p.DateOfBirth, p.Gender, p.ContactNumber))
            .ToListAsync();

        return new PaginatedList<PatientResponse>(patients, totalCount, pageNumber, pageSize);
    }

    // 2. جلب مريض محدد
    public async Task<PatientResponse?> GetPatientByIdAsync(int id)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null) return null;

        return new PatientResponse(patient.Id, patient.UserId, patient.FirstName, patient.LastName, patient.DateOfBirth, patient.Gender, patient.ContactNumber);
    }

    // 3. إضافة مريض جديد
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

    // 4. تعديل بيانات مريض
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

    // 5. حذف مريض
    public async Task<bool> DeletePatientAsync(int id)
    {
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
        if (patient == null) return false;

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return true;
    }
}