using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;

[ApiController]
[Route("api/staff")]
[EnableRateLimiting("GeneralPolicy")]
public sealed class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    // Stores the staff service used by protected staff-management routes.
    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    // Creates a Nurse account through an Admin-only operation.
    [HttpPost("nurses")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNurse(
        [FromBody] CreateNurseRequest request)
    {
        var result = await _staffService.CreateNurseAsync(request);
        if (!result.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Nurse creation failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }

        return StatusCode(StatusCodes.Status201Created, result.Nurse);
    }

    // Returns nurse profiles to Admins and Doctors for care assignments.
    [HttpGet("nurses")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetNurses()
    {
        var nurses = await _staffService.GetNursesAsync();
        return Ok(nurses);
    }

    // Creates a Doctor account through an Admin-only operation.
    [HttpPost("doctors")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDoctor(
        [FromBody] CreateDoctorRequest request)
    {
        var result = await _staffService.CreateDoctorAsync(request);
        if (!result.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Doctor creation failed.",
                detail: result.Message,
                instance: HttpContext.Request.Path);
        }

        return StatusCode(StatusCodes.Status201Created, result.Doctor);
    }

    // Returns Doctor profiles to Admins and authenticated care-team members.
    [HttpGet("doctors")]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _staffService.GetDoctorsAsync();
        return Ok(doctors);
    }
}
