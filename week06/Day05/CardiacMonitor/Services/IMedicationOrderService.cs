using CardiacMonitor.DTOs;

namespace CardiacMonitor.Services;

public interface IMedicationOrderService
{
    // Creates a medication order and updates stock atomically.
    Task<CreateMedicationOrderResult> CreateOrderAsync(
        int patientId,
        CreateMedicationOrderRequest request,
        CancellationToken cancellationToken = default);
}
