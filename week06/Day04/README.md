# Week 6 - Day 4

## Implementing Core Routes II: Write Operations & Business Logic

> Building a transactional medication order workflow with stock validation and calculated totals.

## Overview

On Day 4, we focused on implementing a medication refill order endpoint that performs real business logic instead of a simple database insert. The workflow validates the patient and requested medications, checks medication activity and stock availability, calculates line and order totals, and updates inventory.

The complete operation is handled through a dedicated service and saved within one transaction. This ensures that an order and its stock changes either succeed together or leave the database unchanged when a failure occurs.

---

## Learning Outcomes

During this task, we learned how to:

- Keep business logic inside a service instead of placing it in the controller.
- Validate business rules before modifying tracked entities.
- Use EF Core transactions to keep related write operations atomic.
- Calculate and preserve monetary values using `decimal` price snapshots.
- Map business outcomes to appropriate HTTP status codes.
- Test both successful writes and rejected operations.

---

## Tasks Completed

### 1. Medication Order Endpoint

We added a protected endpoint that allows doctors and administrators to create medication orders for a specific patient.

```csharp
[HttpPost("api/patients/{patientId}/medication-orders")]
[Authorize(Roles = "Admin,Doctor")]
public async Task<IActionResult> Create(
    int patientId,
    [FromBody] CreateMedicationOrderRequest request,
    CancellationToken cancellationToken)
```

The controller converts service results into clear API responses, including `201 Created`, `404 Not Found`, and `409 Conflict`.

### 2. Request and Business Validation

FluentValidation rejects empty orders, duplicate medications, invalid IDs, and non-positive quantities before the service starts processing the request.

```csharp
RuleFor(x => x.Items)
    .NotEmpty().WithMessage("At least one medication item is required.")
    .Must(items => items is null ||
        items.Select(item => item.MedicationId).Distinct().Count() == items.Count)
    .WithMessage("Each medication can appear only once in an order.");
```

The service then verifies that the patient exists and that every requested medication belongs to that patient, is active, and has enough stock.

```csharp
foreach (var requestedItem in request.Items)
{
    var medication = medications[requestedItem.MedicationId];
    if (medication.StockQuantity < requestedItem.Quantity)
    {
        return new(
            CreateMedicationOrderStatus.InsufficientStock,
            Message: $"Insufficient stock for '{medication.Name}'. Available quantity: {medication.StockQuantity}.");
    }
}
```

### 3. Price and Order Total Calculation

Each order item stores the medication price at the time of purchase. This keeps historical orders accurate even if the medication price changes later.

```csharp
var lineTotal = medication.UnitPrice * requestedItem.Quantity;

order.Items.Add(new MedicationOrderItem
{
    MedicationId = medication.Id,
    Quantity = requestedItem.Quantity,
    UnitPrice = medication.UnitPrice,
    LineTotal = lineTotal
});

order.TotalAmount += lineTotal;
```

### 4. Atomic Stock Update with Transactions

The order creation and stock decrement are performed as one atomic operation. Relational database providers use a serializable transaction to protect the stock check and update from competing requests.

```csharp
if (_context.Database.IsRelational())
{
    transaction = await _context.Database.BeginTransactionAsync(
        IsolationLevel.Serializable,
        cancellationToken);
}

medication.StockQuantity -= requestedItem.Quantity;

_context.MedicationOrders.Add(order);
await _context.SaveChangesAsync(cancellationToken);

if (transaction is not null)
{
    await transaction.CommitAsync(cancellationToken);
}
```

If an exception occurs, the transaction is rolled back so that partial order data or incorrect stock values are not persisted.

### 5. Automated Service Tests

We added tests for the main success and failure paths. The successful test confirms total calculations, order persistence, and stock reduction, while the insufficient-stock test confirms that the database remains unchanged.

```csharp
Assert.Equal(CreateMedicationOrderStatus.Created, result.Status);
Assert.Equal(12.00m, result.Order!.TotalAmount);
Assert.Equal(8, (await context.Medications.FindAsync(1))!.StockQuantity);
Assert.Single(context.MedicationOrders);
Assert.Equal(2, context.MedicationOrderItems.Count());
```

---

## Related Files

- `Models/Medication.cs`
- `Models/MedicationOrder.cs`
- `Models/MedicationOrderItem.cs`
- `DTOs/MedicationOrderDtos.cs`
- `Validators/CreateMedicationOrderRequestValidator.cs`
- `Services/IMedicationOrderService.cs`
- `Services/MedicationOrderService.cs`
- `Controllers/MedicationOrdersController.cs`
- `CardiacMonitor.Tests/MedicationOrderServiceTests.cs`
- `Data/Migrations/20260828115119_AddMedicationOrders.cs`

---

## Final Result

The API now supports a complete medication refill order workflow with authorization, request validation, stock checks, price calculations, inventory updates, and automated tests. The controller remains focused on HTTP concerns while the service owns the business rules and transaction boundary.

> This implementation provides a reliable write flow in which the order, its items, and medication stock remain consistent as one unit of work.