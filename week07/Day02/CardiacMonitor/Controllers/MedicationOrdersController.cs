using CardiacMonitor.DTOs;
using CardiacMonitor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitor.Controllers;

[ApiController]
[EnableRateLimiting("GeneralPolicy")]
public class MedicationOrdersController : ControllerBase
{
    private readonly IMedicationOrderService _orderService;

    // Initializes the medication orders controller.
    public MedicationOrdersController(IMedicationOrderService orderService)
    {
        _orderService = orderService;
    }

    // Creates a medication refill order for a patient.
    [HttpPost("api/patients/{patientId}/medication-orders")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Create(
        int patientId,
        [FromBody] CreateMedicationOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateOrderAsync(patientId, request, cancellationToken);

        return result.Status switch
        {
            CreateMedicationOrderStatus.Created => StatusCode(StatusCodes.Status201Created, result.Order),
            CreateMedicationOrderStatus.PatientNotFound or CreateMedicationOrderStatus.MedicationNotFound =>
                NotFound(new { result.Message }),
            CreateMedicationOrderStatus.MedicationInactive or CreateMedicationOrderStatus.InsufficientStock =>
                Conflict(new { result.Message }),
            _ => BadRequest(new { result.Message })
        };
    }
}
