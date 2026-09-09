namespace CardiacMonitor.DTOs;

public record CreateMedicationOrderRequest(
    IReadOnlyCollection<CreateMedicationOrderItemRequest> Items
);

public record CreateMedicationOrderItemRequest(
    int MedicationId,
    int Quantity
);

public record MedicationOrderItemResponse(
    int MedicationId,
    string MedicationName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public record MedicationOrderResponse(
    int Id,
    int PatientId,
    DateTime OrderedAt,
    decimal TotalAmount,
    IReadOnlyCollection<MedicationOrderItemResponse> Items
);

public enum CreateMedicationOrderStatus
{
    Created,
    PatientNotFound,
    MedicationNotFound,
    MedicationInactive,
    InsufficientStock
}

public record CreateMedicationOrderResult(
    CreateMedicationOrderStatus Status,
    MedicationOrderResponse? Order = null,
    string? Message = null
);
