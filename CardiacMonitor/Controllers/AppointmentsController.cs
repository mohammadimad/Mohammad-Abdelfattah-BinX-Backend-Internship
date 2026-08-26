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
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access forbidden.",
                    detail: "Patients can access only their own appointments.",
                    instance: HttpContext.Request.Path);
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
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Appointment creation failed.",
                detail: "Verify that the patient exists and the selected user belongs to the Doctor role.",
                instance: HttpContext.Request.Path);
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
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Appointment not found.",
                detail: $"Appointment with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }

        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        if (isPatient)
        {
            var patient = await _patientService.GetPatientByIdAsync(app.PatientId);
            if (patient == null || patient.UserId != loggedInUserId)
            {
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access forbidden.",
                    detail: "Patients can access only their own appointments.",
                    instance: HttpContext.Request.Path);
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
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Appointment update failed.",
                detail: "Verify that the appointment exists and the selected user belongs to the Doctor role.",
                instance: HttpContext.Request.Path);
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
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Appointment not found.",
                detail: $"Appointment with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }
        return NoContent();
    }
}
