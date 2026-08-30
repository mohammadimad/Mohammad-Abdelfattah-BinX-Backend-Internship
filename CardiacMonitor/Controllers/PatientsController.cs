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
    public async Task<IActionResult> GetAll([FromQuery] PatientQueryParameters queryParameters)
    {
        var patients = await _patientService.GetAllPatientsAsync(queryParameters);
        return Ok(patients);
    }

    // 2. GET: api/patients/{id} (محمي بالفلسفة الأمنية الكاملة)
    [HttpGet("{id}")]
    [Authorize] // مسموح لجميع الأدوار المسجلة الدخول، ولكن الفحص الأمني بالداخل يفصل الصلاحية
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Patient not found.",
                detail: $"Patient with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }

        //Ownership Check: If the user is ill, it must be the same patient attempting to access their data.
        var loggedInUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isPatient = User.IsInRole("Patient");

        // 🛡️ الفلسفة الأمنية (Ownership Check):
        // If the user is sick and tries to read data from another patient other than their own profile -> we will immediately block them with a 403 Forbidden!
        if (isPatient && patient.UserId != loggedInUserId)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access forbidden.",
                detail: "Patients can access only their own profile.",
                instance: HttpContext.Request.Path);
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
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Patient not found.",
                detail: $"Patient with ID {id} was not found.",
                instance: HttpContext.Request.Path);
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
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Patient not found.",
                detail: $"Patient with ID {id} was not found.",
                instance: HttpContext.Request.Path);
        }
        return NoContent();
    }
}
