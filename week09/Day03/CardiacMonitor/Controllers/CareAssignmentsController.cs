using System.Security.Claims;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;

[ApiController]
[EnableRateLimiting("GeneralPolicy")]
public sealed class CareAssignmentsController : ControllerBase
{
    private readonly ICareAssignmentService _careAssignmentService;

    // Stores the care-assignment service used by assignment routes.
    public CareAssignmentsController(
        ICareAssignmentService careAssignmentService)
    {
        _careAssignmentService = careAssignmentService;
    }

    // Assigns a Nurse to a patient through an Admin or Doctor operation.
    [HttpPost("api/patients/{patientId:int}/care-assignments")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> AssignNurse(
        int patientId,
        [FromBody] CreateCareAssignmentRequest request)
    {
        var assignedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(assignedByUserId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid identity claim is required.",
                instance: HttpContext.Request.Path);
        }

        var result = await _careAssignmentService.AssignNurseAsync(
            patientId,
            request,
            assignedByUserId);
        if (!result.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Care assignment failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }

        return StatusCode(StatusCodes.Status201Created, result.Assignment);
    }

    // Returns current and historical nurse assignments for a patient.
    [HttpGet("api/patients/{patientId:int}/care-assignments")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetPatientAssignments(int patientId)
    {
        var assignments = await _careAssignmentService
            .GetPatientAssignmentsAsync(patientId);
        return Ok(assignments);
    }

    // Returns only the patients actively assigned to the current Nurse.
    [HttpGet("api/nurses/me/patients")]
    [Authorize(Roles = "Nurse")]
    public async Task<IActionResult> GetMyPatients()
    {
        var nurseUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(nurseUserId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication required.",
                detail: "A valid identity claim is required.",
                instance: HttpContext.Request.Path);
        }

        var patients = await _careAssignmentService
            .GetAssignedPatientsAsync(nurseUserId);
        return Ok(patients);
    }

    // Ends an active assignment while preserving its audit history.
    [HttpDelete("api/care-assignments/{id:int}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> EndAssignment(int id)
    {
        var ended = await _careAssignmentService.EndAssignmentAsync(id);
        if (!ended)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Active care assignment not found.",
                detail: $"Active care assignment with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }

        return NoContent();
    }
}
