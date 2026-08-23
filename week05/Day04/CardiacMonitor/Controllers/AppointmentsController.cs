using System.Security.Claims;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;

[ApiController]
[EnableRateLimiting("GeneralPolicy")]

public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appService;
    private readonly IPatientService _patientService;

    public AppointmentsController(IAppointmentService appService, IPatientService patientService)
    {
        _appService = appService;
        _patientService = patientService;
    }

    // 1. GET: api/patients/{patientId}/appointments
    // This endpoint retrieves all appointments for a specific patient. Patients can only access their own appointments, while Admins and Doctors can access any patient's appointments.
    [HttpGet("api/patients/{patientId}/appointments")]
    [Authorize]
    public async Task<IActionResult> GetPatientAppointments(int patientId)
    {
        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        if (isPatient)
        {
            var patient = await _patientService.GetPatientByIdAsync(patientId);
            if (patient == null || patient.UserId != loggedInUserId)
            {
                return Forbid();
            }
        }

        var apps = await _appService.GetAppointmentsByPatientIdAsync(patientId);
        return Ok(apps);
    }

    // 2. POST: api/patients/{patientId}/appointments (مسموح فقط للجدولة الطبية للأدمن والأطباء)
    // This endpoint allows Admins and Doctors to create a new appointment for a specific patient. Patients cannot create appointments for themselves or others.
    [HttpPost("api/patients/{patientId}/appointments")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> CreateAppointment(int patientId, [FromBody] CreateAppointmentRequest request)
    {
        var created = await _appService.CreateAppointmentAsync(patientId, request);
        if (created == null)
        {
            return BadRequest(new { Message = "Failed to create appointment. Verify Patient ID and Doctor ID exist." });
        }
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // 3. GET: api/appointments/{id}
    // This endpoint retrieves a specific appointment by its ID. Patients can only access their own appointme nts, while Admins and Doctors can access any appointment.
    [HttpGet("api/appointments/{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var app = await _appService.GetAppointmentByIdAsync(id);
        if (app == null)
        {
            return NotFound(new { Message = $"Appointment with ID {id} was not found." });
        }

        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        if (isPatient)
        {
            var patient = await _patientService.GetPatientByIdAsync(app.PatientId);
            if (patient == null || patient.UserId != loggedInUserId)
            {
                return Forbid();
            }
        }

        return Ok(app);
    }

    // 4. PUT: api/appointments/{id}
    // Doctors and Admins can update appointment details. Patients cannot update their appointments.
    [HttpPut("api/appointments/{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentRequest request)
    {
        var updated = await _appService.UpdateAppointmentAsync(id, request);
        if (!updated)
        {
            return BadRequest(new { Message = "Failed to update. Ensure Appointment ID and Doctor ID are valid." });
        }
        return NoContent();
    }

    // 5. DELETE: api/appointments/{id}
    // Admins and Doctors can delete appointments. Patients cannot delete their appointments.
    [HttpDelete("api/appointments/{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _appService.DeleteAppointmentAsync(id);
        if (!deleted)
        {
            return NotFound(new { Message = $"Appointment with ID {id} was not found." });
        }
        return NoContent();
    }
}