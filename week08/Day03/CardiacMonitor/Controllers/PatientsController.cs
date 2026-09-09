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
    //[HttpGet("api/test-n-plus-one")]
    //[AllowAnonymous]
    //public async Task<IActionResult> TestNPlusOneProblem([FromServices] AppDbContext context)
    //{
    //    // Query 1: جلب 10 مرضى
    //    var patients = await context.Patients.Take(10).ToListAsync();

    //    var result = new List<object>();

    //    foreach (var patient in patients)
    //    {
    //        // الكارثة (N Queries): استعلام قاعدة البيانات داخل حلقة تكرار (Loop)!
    //        var vitals = await context.VitalSigns
    //                                  .Where(v => v.PatientId == patient.Id)
    //                                  .ToListAsync();

    //        result.Add(new { PatientName = patient.FirstName, VitalsCount = vitals.Count });
    //    }

    //    return Ok(result);
    //}
    //Performance Optimization: Eager Loading with .Include

    //Fixed N+1 Problem: Eager Loading with .Include
    [HttpGet("fixed-performance")]
    public async Task<IActionResult> GetFixed() => Ok(await _patientService.GetFixedPerformanceAsync());
    // [GET] -> /api/patients/{id}/full-dashboard

    [HttpGet("{id}/full-dashboard")]
    public async Task<IActionResult> GetDashboard(int id)
    {
        var result = await _patientService.GetFullDashboardAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

        //Performance Optimization: Projection
        // [GET] -> /api/patients/optimized-projection
        [HttpGet("optimized-projection")]
    public async Task<IActionResult> GetProjection() => Ok(await _patientService.GetPatientsWithProjectionAsync());

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
