using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;

[ApiController]
[EnableRateLimiting("GeneralPolicy")]
public sealed class DoctorsController : ControllerBase
{
    private readonly IDoctorScheduleService _doctorScheduleService;

    // Stores the Doctor schedule service used by availability routes.
    public DoctorsController(IDoctorScheduleService doctorScheduleService)
    {
        _doctorScheduleService = doctorScheduleService;
    }

    // Creates a weekly availability slot for an Admin or the owning Doctor.
    [HttpPost("api/doctors/{doctorProfileId:int}/availability")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> AddAvailability(
        int doctorProfileId,
        [FromBody] CreateDoctorAvailabilityRequest request)
    {
        if (!await _doctorScheduleService.CanManageDoctorAsync(
                User,
                doctorProfileId))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access forbidden.",
                detail: "You cannot manage this Doctor schedule.",
                instance: HttpContext.Request.Path);
        }

        var result = await _doctorScheduleService.AddAvailabilityAsync(
            doctorProfileId,
            request);
        if (!result.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Availability creation failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }

        return StatusCode(StatusCodes.Status201Created, result.Availability);
    }

    // Returns the active weekly schedule for a Doctor profile.
    [HttpGet("api/doctors/{doctorProfileId:int}/availability")]
    [Authorize(Roles = "Admin,Doctor,Nurse,Patient")]
    public async Task<IActionResult> GetAvailability(int doctorProfileId)
    {
        var availability = await _doctorScheduleService
            .GetAvailabilityAsync(doctorProfileId);
        return Ok(availability);
    }

    // Deactivates a schedule slot for an Admin or the owning Doctor.
    [HttpDelete("api/doctor-availability/{id:int}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> DeactivateAvailability(int id)
    {
        var slot = await _doctorScheduleService.GetAvailabilityByIdAsync(id);
        if (slot == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Availability not found.",
                detail: $"Doctor availability with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }

        if (!await _doctorScheduleService.CanManageDoctorAsync(
                User,
                slot.DoctorProfileId))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access forbidden.",
                detail: "You cannot manage this Doctor schedule.",
                instance: HttpContext.Request.Path);
        }

        await _doctorScheduleService.DeactivateAvailabilityAsync(id);
        return NoContent();
    }
}
