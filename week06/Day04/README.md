# Day 4 - Write Operations, Business Logic, and Code Review

## Objective

Day 4 extends the Cardiac Monitor API with a medication refill order workflow that contains real business logic rather than simple CRUD. A valid order must check medication availability, calculate immutable price totals, persist the order and its items, and decrement stock as one atomic operation.

## Requirement mapping

| Curriculum requirement | Project implementation |
| --- | --- |
| Check stock before creating an order | `MedicationOrderService` rejects any item whose requested quantity exceeds the available stock |
| Reject invalid medication requests | Missing medications return `404 Not Found`; inactive or insufficiently stocked medications return `409 Conflict` |
| Calculate line totals | Each line stores `UnitPrice` and `LineTotal = UnitPrice * Quantity` |
| Calculate the order total | `MedicationOrder.TotalAmount` is accumulated from all line totals |
| Decrement stock | Each medication's `StockQuantity` is reduced only after every requested item passes validation |
| Use one database transaction | Relational providers use an EF Core transaction with `IsolationLevel.Serializable` |
| Cover success and failure paths | `MedicationOrderServiceTests` verifies successful persistence and insufficient-stock rejection |

## Added domain model

### Medication changes

The existing `Medication` entity now includes:

- `StockQuantity`: current available units.
- `UnitPrice`: price captured when an order is created.
- `OrderItems`: navigation collection for historical order lines.

### MedicationOrder

Represents the order header:

| Property | Purpose |
| --- | --- |
| `Id` | Primary key |
| `PatientId` | Patient who owns the order |
| `OrderedAt` | UTC creation time |
| `TotalAmount` | Sum of all line totals |
| `Items` | Collection of order lines |

### MedicationOrderItem

Represents one requested medication:

| Property | Purpose |
| --- | --- |
| `MedicationOrderId` | Parent order foreign key |
| `MedicationId` | Ordered medication foreign key |
| `Quantity` | Requested quantity |
| `UnitPrice` | Price snapshot at order time |
| `LineTotal` | `UnitPrice * Quantity` |

Saving `UnitPrice` on the order item is intentional. A future change to the medication's current price must not rewrite the financial history of an existing order.

## API endpoint

```http
POST /api/patients/{patientId}/medication-orders
Authorization: Bearer <doctor-or-admin-token>
Content-Type: application/json
```

Only users in the `Admin` or `Doctor` role can create an order.

Example request:

```json
{
  "items": [
    {
      "medicationId": 1,
      "quantity": 2
    },
    {
      "medicationId": 2,
      "quantity": 1
    }
  ]
}
```

Example success response:

```http
HTTP/1.1 201 Created
```

```json
{
  "id": 15,
  "patientId": 1,
  "orderedAt": "2026-08-28T09:30:00Z",
  "totalAmount": 12.00,
  "items": [
    {
      "medicationId": 1,
      "medicationName": "Aspirin",
      "quantity": 2,
      "unitPrice": 2.50,
      "lineTotal": 5.00
    },
    {
      "medicationId": 2,
      "medicationName": "Statin",
      "quantity": 1,
      "unitPrice": 7.00,
      "lineTotal": 7.00
    }
  ]
}
```

## Response behavior

| Situation | Status code | Database effect |
| --- | ---: | --- |
| Valid order | `201 Created` | Order and items are inserted; stock is decremented |
| Patient does not exist | `404 Not Found` | No changes |
| Medication does not exist or belongs to another patient | `404 Not Found` | No changes |
| Medication is inactive | `409 Conflict` | No changes |
| Stock is insufficient | `409 Conflict` | No changes |
| Request has no items, duplicates, or non-positive values | `400 Bad Request` | FluentValidation rejects the request before the service runs |
| Unexpected failure | `500 Internal Server Error` | The transaction is rolled back and middleware returns safe `ProblemDetails` |

## Transaction boundary

`MedicationOrderService.CreateOrderAsync` performs the complete write flow inside one transaction when the configured provider is relational:

1. Begin a serializable transaction.
2. Confirm that the patient exists.
3. Load all requested medications owned by that patient.
4. Reject missing, inactive, or insufficiently stocked items.
5. Create the order and calculate every total.
6. Decrement medication stock.
7. Save all changes once.
8. Commit on success or roll back after an exception.

Serializable isolation protects the stock check and update from competing orders that attempt to buy the last available units. The EF Core InMemory provider does not support relational transactions, so automated unit tests exercise the same service without opening a transaction; the SQL Server path uses the real transaction.

## Database migration

The migration `20260828115119_AddMedicationOrders` adds:

- `StockQuantity` and `UnitPrice` to `Medications`.
- `MedicationOrders` and `MedicationOrderItems`.
- Foreign-key indexes and delete behaviors.
- Decimal precision for monetary values.
- `CK_Medications_StockQuantity`, preventing negative stock at the database level.

Apply the migration from the project directory:

```powershell
dotnet ef database update
```

## Automated tests

The relevant test class is `CardiacMonitor.Tests/MedicationOrderServiceTests.cs`.

It verifies:

- A valid multi-item order calculates totals, persists its rows, and decrements stock.
- An insufficient-stock order leaves medications, orders, and order items unchanged.

Run the full solution tests:

```powershell
dotnet test .\CardiacMonitor.slnx --configuration Release
```

## Code-review checklist

- [x] Business logic is placed in a service rather than the controller.
- [x] Request and response DTOs define the public API contract.
- [x] The stock check runs before any entity is modified.
- [x] Monetary values use `decimal` with database precision.
- [x] The relational write flow has one transaction boundary.
- [x] Expected business failures use explicit HTTP status codes.
- [x] Happy-path and insufficient-stock tests pass locally.
- [ ] Pull request reviewed and approved by the mentor.
- [ ] SQL Server concurrency behavior demonstrated with competing last-unit orders.

The unchecked items require external review or a real relational concurrency run and must not be reported as completed before evidence exists.

