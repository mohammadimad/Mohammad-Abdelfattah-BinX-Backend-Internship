using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public sealed class StaffService : IStaffService
{
    private const string NurseRole = "Nurse";
    private const string DoctorRole = "Doctor";
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    // Stores the Identity managers and database context used by staff operations.
    public StaffService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // Creates a Nurse identity and linked professional profile atomically.
    public async Task<NurseCreationResult> CreateNurseAsync(
        CreateNurseRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
        {
            return new NurseCreationResult(false, "Email is already registered.");
        }

        if (await _context.NurseProfiles.AnyAsync(nurse =>
                nurse.LicenseNumber == request.LicenseNumber))
        {
            return new NurseCreationResult(false, "License number is already registered.");
        }

        if (!await _roleManager.RoleExistsAsync(NurseRole))
        {
            return new NurseCreationResult(false, "The Nurse role is not configured.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };
            var creationResult = await _userManager.CreateAsync(user, request.Password);
            if (!creationResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = string.Join(", ", creationResult.Errors.Select(error =>
                    error.Description));
                return new NurseCreationResult(false, $"Nurse creation failed: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, NurseRole);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = string.Join(", ", roleResult.Errors.Select(error =>
                    error.Description));
                return new NurseCreationResult(false, $"Nurse role assignment failed: {errors}");
            }

            var nurse = new NurseProfile
            {
                UserId = user.Id,
                FullName = request.FullName.Trim(),
                LicenseNumber = request.LicenseNumber.Trim(),
                Department = request.Department.Trim()
            };
            _context.NurseProfiles.Add(nurse);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new NurseCreationResult(
                true,
                "Nurse account created successfully.",
                new NurseResponse(
                    nurse.Id,
                    nurse.UserId,
                    user.Email!,
                    nurse.FullName,
                    nurse.LicenseNumber,
                    nurse.Department));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Returns the nurse profiles available for care assignments.
    public async Task<IReadOnlyList<NurseResponse>> GetNursesAsync()
    {
        return await _context.NurseProfiles
            .AsNoTracking()
            .OrderBy(nurse => nurse.FullName)
            .ThenBy(nurse => nurse.Id)
            .Select(nurse => new NurseResponse(
                nurse.Id,
                nurse.UserId,
                nurse.User.Email!,
                nurse.FullName,
                nurse.LicenseNumber,
                nurse.Department))
            .ToListAsync();
    }

    // Creates a Doctor identity and linked professional profile atomically.
    public async Task<DoctorCreationResult> CreateDoctorAsync(
        CreateDoctorRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
        {
            return new DoctorCreationResult(false, "Email is already registered.");
        }

        if (await _context.DoctorProfiles.AnyAsync(doctor =>
                doctor.LicenseNumber == request.LicenseNumber))
        {
            return new DoctorCreationResult(false, "License number is already registered.");
        }

        if (!await _roleManager.RoleExistsAsync(DoctorRole))
        {
            return new DoctorCreationResult(false, "The Doctor role is not configured.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };
            var creationResult = await _userManager.CreateAsync(user, request.Password);
            if (!creationResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = string.Join(", ", creationResult.Errors.Select(error =>
                    error.Description));
                return new DoctorCreationResult(false, $"Doctor creation failed: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, DoctorRole);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                var errors = string.Join(", ", roleResult.Errors.Select(error =>
                    error.Description));
                return new DoctorCreationResult(false, $"Doctor role assignment failed: {errors}");
            }

            var doctor = new DoctorProfile
            {
                UserId = user.Id,
                FullName = request.FullName.Trim(),
                LicenseNumber = request.LicenseNumber.Trim(),
                Specialty = request.Specialty.Trim(),
                Department = request.Department.Trim(),
                IsActive = true
            };
            _context.DoctorProfiles.Add(doctor);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new DoctorCreationResult(
                true,
                "Doctor account created successfully.",
                new DoctorResponse(
                    doctor.Id,
                    doctor.UserId,
                    user.Email!,
                    doctor.FullName,
                    doctor.LicenseNumber,
                    doctor.Specialty,
                    doctor.Department,
                    doctor.IsActive));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Returns active and inactive Doctor professional profiles.
    public async Task<IReadOnlyList<DoctorResponse>> GetDoctorsAsync()
    {
        return await _context.DoctorProfiles
            .AsNoTracking()
            .OrderByDescending(doctor => doctor.IsActive)
            .ThenBy(doctor => doctor.FullName)
            .ThenBy(doctor => doctor.Id)
            .Select(doctor => new DoctorResponse(
                doctor.Id,
                doctor.UserId,
                doctor.User.Email!,
                doctor.FullName,
                doctor.LicenseNumber,
                doctor.Specialty,
                doctor.Department,
                doctor.IsActive))
            .ToListAsync();
    }
}
