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
    private readonly IPatientAccessService _patientAccessService;

    public PatientsController(
        IPatientService patientService,
        IPatientAccessService patientAccessService)
    {
        _patientService = patientService;
        _patientAccessService = patientAccessService;
    }

    // 1. GET: api/patient
    // accessible only to Admins and Doctors to retrieve all patients
    /// <summary>Lists Patients with database-side search, filtering, sorting, and pagination.</summary>
    /// <param name="queryParameters">Page and page size, optional search and gender, and a supported sort key.</param>
    /// <response code="200">A page of Patient profiles with total count and page metadata.</response>
    /// <response code="400">Invalid paging or filtering parameters.</response>
    /// <response code="401">A valid bearer token is required.</response>
    /// <response code="403">Only Admins and Doctors can list all Patients.</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> GetAll([FromQuery] PatientQueryParameters queryParameters)
    {
        var patients = await _patientService.GetAllPatientsAsync(queryParameters);
        return Ok(patients);
    }

    // 2. GET: api/patients/{id} (محمي بالفلسفة الأمنية الكاملة)
    /// <summary>Reads a Patient profile after role and resource-access checks.</summary>
    /// <param name="id">The numeric Patient profile ID, not an Identity user ID.</param>
    /// <response code="200">The requested Patient profile.</response>
    /// <response code="403">A Patient does not own the profile or a Nurse has no active assignment.</response>
    /// <response code="404">The Patient profile does not exist.</response>
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

    // Returns a single patient's related clinical collections for a detail view.
    /// <summary>Reads a Patient and its clinical collections using EF Core split queries.</summary>
    /// <param name="id">The Patient whose vitals, medications, appointments, alerts, and care assignments are requested.</param>
    /// <response code="200">The profile and related clinical collections.</response>
    /// <response code="403">The caller cannot access this Patient.</response>
    /// <response code="404">The Patient does not exist.</response>
    [HttpGet("{id}/clinical-details")]
    [Authorize(Roles = "Admin,Doctor,Patient,Nurse")]
    public async Task<IActionResult> GetClinicalDetails(int id)
    {
        var patient = await _patientService.GetClinicalDetailsAsync(id);
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
