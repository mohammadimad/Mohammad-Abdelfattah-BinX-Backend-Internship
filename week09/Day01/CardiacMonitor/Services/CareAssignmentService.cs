using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Services;

public sealed class CareAssignmentService : ICareAssignmentService
{
    private readonly AppDbContext _context;

    // Stores the database context used by care-assignment operations.
    public CareAssignmentService(AppDbContext context)
    {
        _context = context;
    }

    // Creates or reactivates a nurse assignment for a patient.
    public async Task<CareAssignmentResult> AssignNurseAsync(
        int patientId,
        CreateCareAssignmentRequest request,
        string assignedByUserId)
    {
        if (!await _context.Patients.AnyAsync(patient => patient.Id == patientId))
        {
            return new CareAssignmentResult(false, "Patient was not found.");
        }

        var nurse = await _context.NurseProfiles
            .FirstOrDefaultAsync(profile => profile.Id == request.NurseProfileId);
        if (nurse == null)
        {
            return new CareAssignmentResult(false, "Nurse profile was not found.");
        }

        var assignment = await _context.PatientCareAssignments
            .FirstOrDefaultAsync(item =>
                item.PatientId == patientId &&
                item.NurseProfileId == request.NurseProfileId);

        if (assignment?.IsActive == true)
        {
            return new CareAssignmentResult(
                false,
                "This nurse is already assigned to the patient.");
        }

        var assignedAt = DateTime.UtcNow;
        if (assignment == null)
        {
            assignment = new PatientCareAssignment
            {
                PatientId = patientId,
                NurseProfileId = request.NurseProfileId
            };
            _context.PatientCareAssignments.Add(assignment);
        }

        assignment.AssignedByUserId = assignedByUserId;
        assignment.AssignedAt = assignedAt;
        assignment.EndedAt = null;
        assignment.IsActive = true;
        assignment.Notes = request.Notes?.Trim();

        await _context.SaveChangesAsync();

        return new CareAssignmentResult(
            true,
            "Nurse assigned to patient successfully.",
            ToResponse(assignment, nurse.FullName));
    }

    // Returns every current and historical assignment for a patient.
    public async Task<IReadOnlyList<CareAssignmentResponse>> GetPatientAssignmentsAsync(
        int patientId)
    {
        return await _context.PatientCareAssignments
            .AsNoTracking()
            .Where(assignment => assignment.PatientId == patientId)
            .OrderByDescending(assignment => assignment.IsActive)
            .ThenByDescending(assignment => assignment.AssignedAt)
            .Select(assignment => new CareAssignmentResponse(
                assignment.Id,
                assignment.PatientId,
                assignment.NurseProfileId,
                assignment.NurseProfile.FullName,
                assignment.AssignedByUserId,
                assignment.AssignedAt,
                assignment.EndedAt,
                assignment.IsActive,
                assignment.Notes))
            .ToListAsync();
    }

    // Returns the active patients assigned to the current nurse identity.
    public async Task<IReadOnlyList<PatientResponse>> GetAssignedPatientsAsync(
        string nurseUserId)
    {
        return await _context.PatientCareAssignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.IsActive &&
                assignment.NurseProfile.UserId == nurseUserId)
            .OrderBy(assignment => assignment.Patient.FirstName)
            .ThenBy(assignment => assignment.Patient.LastName)
            .ThenBy(assignment => assignment.PatientId)
            .Select(assignment => new PatientResponse(
                assignment.Patient.Id,
                assignment.Patient.UserId,
                assignment.Patient.FirstName,
                assignment.Patient.LastName,
                assignment.Patient.DateOfBirth,
                assignment.Patient.Gender,
                assignment.Patient.ContactNumber))
            .ToListAsync();
    }

    // Ends an active care assignment without deleting its audit history.
    public async Task<bool> EndAssignmentAsync(int assignmentId)
    {
        var assignment = await _context.PatientCareAssignments
            .FirstOrDefaultAsync(item => item.Id == assignmentId);
        if (assignment == null || !assignment.IsActive)
        {
            return false;
        }

        assignment.IsActive = false;
        assignment.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Checks whether a Nurse identity has an active assignment to a patient.
    public async Task<bool> IsNurseAssignedAsync(
        string nurseUserId,
        int patientId)
    {
        return await _context.PatientCareAssignments.AnyAsync(assignment =>
            assignment.PatientId == patientId &&
            assignment.IsActive &&
            assignment.NurseProfile.UserId == nurseUserId);
    }

    // Maps a care-assignment entity and nurse name to its API response.
    private static CareAssignmentResponse ToResponse(
        PatientCareAssignment assignment,
        string nurseName)
    {
        return new CareAssignmentResponse(
            assignment.Id,
            assignment.PatientId,
            assignment.NurseProfileId,
            nurseName,
            assignment.AssignedByUserId,
            assignment.AssignedAt,
            assignment.EndedAt,
            assignment.IsActive,
            assignment.Notes);
    }
}
