using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Controllers;
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("GeneralPolicy")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IPatientAccessService _patientAccessService;

    public PatientsController(
        IPatientService patientService,
        IPatientAccessService patientAccessService)
    {
        _patientService = patientService;
        _patientAccessService = patientAccessService;
    }
    [HttpGet("api/test-n-plus-one")]
    [AllowAnonymous]
    public async Task<IActionResult> TestNPlusOneProblem([FromServices] AppDbContext context)
    {
        // Query 1: Bring in 10 patients
        var patients = await context.Patients.Take(10).ToListAsync();

        var result = new List<object>();

        foreach (var patient in patients)
        {
            // Disaster (N Queries): A database query inside a loop!
            var vitals = await context.VitalSigns
                                      .Where(v => v.PatientId == patient.Id)
                                      .ToListAsync();

            result.Add(new { PatientName = patient.FirstName, VitalsCount = vitals.Count });
        }

        return Ok(result);
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
    [Authorize(Roles = "Admin,Doctor,Patient,Nurse")]
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

        if (!await _patientAccessService.CanAccessPatientAsync(User, id))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access forbidden.",
                detail: "You do not have access to this patient profile.",
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
