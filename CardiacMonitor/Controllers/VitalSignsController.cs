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
    private readonly IPatientAccessService _patientAccessService;
    public VitalSignsController(
        IVitalSignService vitalService,
        IPatientAccessService patientAccessService)
    {
        _vitalService = vitalService;
        _patientAccessService = patientAccessService;
    }

    // 1. GET: api/patients/{patientId}/vitals
    //Patient can only access their own vital signs, while Admins and Doctors can access any patient's vital signs.
    [HttpGet("api/patients/{patientId}/vitals")]
    [Authorize(Roles = "Admin,Doctor,Patient,Nurse")]
    public async Task<IActionResult> GetPatientVitals(
        int patientId,
        [FromQuery] VitalSignQueryParameters queryParameters)
    {
        if (!await _patientAccessService.CanAccessPatientAsync(User, patientId))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access forbidden.",
                detail: "You do not have access to this patient's vital signs.",
                instance: HttpContext.Request.Path);
        }

        var vitals = await _vitalService.GetVitalSignsByPatientIdAsync(
            patientId,
            queryParameters);
        return Ok(vitals);
    }

    // 2. POST: api/patients/{patientId}/vitals 
    // Patients can only create vital signs for themselves, while Admins and Doctors can create vital signs for any patient.
    /// <summary>Records vital signs and creates a medical alert when a critical threshold is crossed.</summary>
    /// <param name="patientId">The accessible Patient profile to receive the reading.</param>
    /// <param name="request">Heart rate in bpm, oxygen saturation in percent, and blood pressure in mmHg.</param>
    /// <response code="201">The reading was saved; a critical reading may also create an alert.</response>
    /// <response code="400">One or more measurement values failed validation.</response>
    /// <response code="403">The caller cannot access this Patient.</response>
    /// <response code="404">The Patient does not exist.</response>
    [HttpPost("api/patients/{patientId}/vitals")]
    [Authorize(Roles = "Admin,Doctor,Patient,Nurse")]
    public async Task<IActionResult> CreateVital(int patientId, [FromBody] CreateVitalSignRequest request)
    {
        if (!await _patientAccessService.CanAccessPatientAsync(User, patientId))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access forbidden.",
                detail: "You cannot create vital signs for this patient.",
                instance: HttpContext.Request.Path);
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
    [Authorize(Roles = "Admin,Doctor,Patient,Nurse")]
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

        if (!await _patientAccessService.CanAccessPatientAsync(User, vital.PatientId))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access forbidden.",
                detail: "You do not have access to this patient's vital signs.",
                instance: HttpContext.Request.Path);
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
