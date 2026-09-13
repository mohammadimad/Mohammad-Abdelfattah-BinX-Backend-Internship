using System.Security.Claims;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;

[ApiController]
[EnableRateLimiting("GeneralPolicy")]
public sealed class MedicalAlertsController : ControllerBase
{
    private readonly IMedicalAlertService _medicalAlertService;
    private readonly IPatientAccessService _patientAccessService;

    // Stores the alert and patient-access services used by protected routes.
    public MedicalAlertsController(
        IMedicalAlertService medicalAlertService,
        IPatientAccessService patientAccessService)
    {
        _medicalAlertService = medicalAlertService;
        _patientAccessService = patientAccessService;
    }

    // Returns alerts only when the user may access the selected patient.
    [HttpGet("api/patients/{patientId:int}/alerts")]
    [Authorize(Roles = "Admin,Doctor,Nurse,Patient")]
    public async Task<IActionResult> GetPatientAlerts(int patientId)
    {
        if (!await _patientAccessService.CanAccessPatientAsync(User, patientId))
        {
            return AccessForbidden();
        }

        var alerts = await _medicalAlertService.GetPatientAlertsAsync(patientId);
        return Ok(alerts);
    }

    // Returns one alert after enforcing access to its patient resource.
    [HttpGet("api/alerts/{id:int}")]
    [Authorize(Roles = "Admin,Doctor,Nurse,Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var alert = await _medicalAlertService.GetByIdAsync(id);
        if (alert == null)
        {
            return AlertNotFound(id);
        }

        if (!await _patientAccessService.CanAccessPatientAsync(
                User,
                alert.PatientId))
        {
            return AccessForbidden();
        }

        return Ok(alert);
    }

    // Allows authorized clinical staff to acknowledge an accessible alert.
    [HttpPatch("api/alerts/{id:int}/acknowledge")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var alert = await _medicalAlertService.GetByIdAsync(id);
        if (alert == null)
        {
            return AlertNotFound(id);
        }

        if (!await _patientAccessService.CanAccessPatientAsync(
                User,
                alert.PatientId))
        {
            return AccessForbidden();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _medicalAlertService.AcknowledgeAsync(id, userId);
        if (!result.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Alert acknowledgement failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Alert);
    }

    // Allows an Admin or Doctor to resolve an active medical alert.
    [HttpPatch("api/alerts/{id:int}/resolve")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Resolve(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _medicalAlertService.ResolveAsync(id, userId);
        if (!result.Succeeded)
        {
            var statusCode = result.Alert == null
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status409Conflict;
            return Problem(
                statusCode: statusCode,
                title: "Alert resolution failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }

        return Ok(result.Alert);
    }

    // Creates a standardized forbidden response for patient-resource access.
    private ObjectResult AccessForbidden()
    {
        return Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Access forbidden.",
            detail: "You do not have access to this patient's medical alerts.",
            instance: HttpContext.Request.Path);
    }

    // Creates a standardized not-found response for an alert identifier.
    private ObjectResult AlertNotFound(int id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Medical alert not found.",
            detail: $"Medical alert with ID {id} was not found.",
            instance: HttpContext.Request.Path);
    }
}
