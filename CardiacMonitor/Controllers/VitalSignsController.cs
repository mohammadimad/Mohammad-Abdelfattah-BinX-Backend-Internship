using System.Security.Claims;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;

[ApiController]
[EnableRateLimiting("GeneralPolicy")]

public class VitalSignsController : ControllerBase
{
    private readonly IVitalSignService _vitalService;
    private readonly IPatientService _patientService; 
    public VitalSignsController(IVitalSignService vitalService, IPatientService patientService)
    {
        _vitalService = vitalService;
        _patientService = patientService;
    }

    // 1. GET: api/patients/{patientId}/vitals
    //Patient can only access their own vital signs, while Admins and Doctors can access any patient's vital signs.
    [HttpGet("api/patients/{patientId}/vitals")]
    [Authorize] 
    public async Task<IActionResult> GetPatientVitals(int patientId)
    {
        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        if (isPatient)
        {
            var patient = await _patientService.GetPatientByIdAsync(patientId);
            // Ownership Check: If the user is a patient, they can only access their own vital signs.
            if (patient == null || patient.UserId != loggedInUserId)
            {
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access forbidden.",
                    detail: "Patients can access only their own vital signs.",
                    instance: HttpContext.Request.Path);
            }
        }

        var vitals = await _vitalService.GetVitalSignsByPatientIdAsync(patientId);
        return Ok(vitals);
    }

    // 2. POST: api/patients/{patientId}/vitals 
    // Patients can only create vital signs for themselves, while Admins and Doctors can create vital signs for any patient.
    [HttpPost("api/patients/{patientId}/vitals")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    public async Task<IActionResult> CreateVital(int patientId, [FromBody] CreateVitalSignRequest request)
    {
        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        if (isPatient)
        {
            var patient = await _patientService.GetPatientByIdAsync(patientId);
            // Ownership Check: If the user is a patient, they can only create vital signs for themselves.
            if (patient == null || patient.UserId != loggedInUserId)
            {
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access forbidden.",
                    detail: "Patients can create vital signs only for themselves.",
                    instance: HttpContext.Request.Path);
            }
        }

        var created = await _vitalService.CreateVitalSignAsync(patientId, request);
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

    // 3. GET: api/vitals/{id}
    // Patients can only access their own vital signs, while Admins and Doctors can access any vital sign.
    [HttpGet("api/vitals/{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var vital = await _vitalService.GetVitalSignByIdAsync(id);
        if (vital == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Vital sign not found.",
                detail: $"Vital sign with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }

        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        if (isPatient)
        {
            var patient = await _patientService.GetPatientByIdAsync(vital.PatientId);
            if (patient == null || patient.UserId != loggedInUserId)
            {
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access forbidden.",
                    detail: "Patients can access only their own vital signs.",
                    instance: HttpContext.Request.Path);
            }
        }

        return Ok(vital);
    }

    // 4. PUT: api/vitals/{id} 
    // Patients are not allowed to update vital signs.
    [HttpPut("api/vitals/{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVitalSignRequest request)
    {
        var updated = await _vitalService.UpdateVitalSignAsync(id, request);
        if (!updated)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Vital sign not found.",
                detail: $"Vital sign with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }
        return NoContent();
    }

    // 5. DELETE: api/vitals/{id} 
    // Patients are not allowed to delete vital signs.
    [HttpDelete("api/vitals/{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _vitalService.DeleteVitalSignAsync(id);
        if (!deleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Vital sign not found.",
                detail: $"Vital sign with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }
        return NoContent();
    }
}
