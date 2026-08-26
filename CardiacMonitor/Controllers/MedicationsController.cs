using System.Security.Claims;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;

[ApiController]
[EnableRateLimiting("GeneralPolicy")]

public class MedicationsController : ControllerBase
{
    private readonly IMedicationService _medService;
    private readonly IPatientService _patientService;

    public MedicationsController(IMedicationService medService, IPatientService patientService)
    {
        _medService = medService;
        _patientService = patientService;
    }

    // 1. GET: api/patients/{patientId}/medications
    // This endpoint retrieves all medications for a specific patient. Patients can only access their own medications, while Admins and Doctors can access any patient's medications.
    [HttpGet("api/patients/{patientId}/medications")]
    [Authorize]
    public async Task<IActionResult> GetPatientMedications(int patientId)
    {
        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        if (isPatient)
        {
            var patient = await _patientService.GetPatientByIdAsync(patientId);
            if (patient == null || patient.UserId != loggedInUserId)
            {
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access forbidden.",
                    detail: "Patients can access only their own medications.",
                    instance: HttpContext.Request.Path);
            }
        }

        var meds = await _medService.GetMedicationsByPatientIdAsync(patientId);
        return Ok(meds);
    }

    // 2. POST: api/patients/{patientId}/medications
    // This endpoint allows Admins and Doctors to create a new medication for a specific patient. Patients cannot create medications for themselves or others.
    [HttpPost("api/patients/{patientId}/medications")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> CreateMedication(int patientId, [FromBody] CreateMedicationRequest request)
    {
        var created = await _medService.CreateMedicationAsync(patientId, request);
        if (created == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Patient not found.",
                detail: $"Patient with ID {patientId} was not found.",
                instance: HttpContext.Request.Path);
        }
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // 3. GET: api/medications/{id}
    // This endpoint retrieves a specific medication by its ID. Patients can only access their own medications, while Admins and Doctors can access any medication.
    [HttpGet("api/medications/{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var med = await _medService.GetMedicationByIdAsync(id);
        if (med == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Medication not found.",
                detail: $"Medication with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }

        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        if (isPatient)
        {
            var patient = await _patientService.GetPatientByIdAsync(med.PatientId);
            if (patient == null || patient.UserId != loggedInUserId)
            {
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access forbidden.",
                    detail: "Patients can access only their own medications.",
                    instance: HttpContext.Request.Path);
            }
        }

        return Ok(med);
    }

    // 4. PUT: api/medications/{id}
    // Only Admins and Doctors can update medications
    [HttpPut("api/medications/{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicationRequest request)
    {
        var updated = await _medService.UpdateMedicationAsync(id, request);
        if (!updated)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Medication not found.",
                detail: $"Medication with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }
        return NoContent();
    }

    // 5. DELETE: api/medications/{id}
    // Only Admins and Doctors can delete medications
    [HttpDelete("api/medications/{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _medService.DeleteMedicationAsync(id);
        if (!deleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Medication not found.",
                detail: $"Medication with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }
        return NoContent();
    }
}
