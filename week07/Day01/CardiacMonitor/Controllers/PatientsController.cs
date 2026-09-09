using System.Security.Claims;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("GeneralPolicy")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // 1. GET: api/patient
    // accessible only to Admins and Doctors to retrieve all patients
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetAll(
       [FromQuery] int pageNumber = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] string? searchName = null,
       [FromQuery] string? gender = null,
       [FromQuery] string? sortBy = null,
       [FromQuery] bool isDescending = false)
    {
         if (pageSize <= 0 || pageSize > 50) pageSize = 10;
        if (pageNumber <= 0) pageNumber = 1;

        var result = await _patientService.GetPaginatedPatientsAsync(
            pageNumber, pageSize, searchName, gender, sortBy, isDescending);

        return Ok(result);
    }
    // 2. GET: api/patients/{id}  
    [HttpGet("{id}")]
    [Authorize] 
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return NotFound(new { Message = $"Patient with ID {id} was not found." });
        }

        //Ownership Check: If the user is ill, it must be the same patient attempting to access their data.
        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        // 🛡️ الفلسفة الأمنية (Ownership Check):
        // If the user is sick and tries to read data from another patient other than their own profile -> we will immediately block them with a 403 Forbidden!
        if (isPatient && patient.UserId != loggedInUserId)
        {
            return Forbid(); 
        }

        return Ok(patient);
    }
    //3. POST: api/patients 
    //Admin can create a new patient record
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
    {
        var createdPatient = await _patientService.CreatePatientAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdPatient.Id }, createdPatient);
    }

    // 4. PUT: api/patients/{id} 
    //Admin and Doctor can update patient information, but not the patient themselves.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientRequest request)
    {
        var updated = await _patientService.UpdatePatientAsync(id, request);
        if (!updated)
        {
            return NotFound(new { Message = $"Patient with ID {id} was not found." });
        }
        return NoContent();
    }

    // 5. DELETE: api/patients/{id} 
    // Admin can delete a patient record
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _patientService.DeletePatientAsync(id);
        if (!deleted)
        {
            return NotFound(new { Message = $"Patient with ID {id} was not found." });
        }
        return NoContent();
    }
}