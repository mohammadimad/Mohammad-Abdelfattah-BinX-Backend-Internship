using System.Data;
using CardiacMonitor.Data;
using CardiacMonitor.DTOs;
using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CardiacMonitor.Services;

public class MedicationOrderService : IMedicationOrderService
{
    private readonly AppDbContext _context;

    // Initializes the medication order service.
    public MedicationOrderService(AppDbContext context)
    {
        _context = context;
    }

    // Creates an order, calculates totals, and decrements stock in one transaction.
    public async Task<CreateMedicationOrderResult> CreateOrderAsync(
        int patientId,
        CreateMedicationOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        IDbContextTransaction? transaction = null;

        if (_context.Database.IsRelational())
        {
            transaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
        }

        try
        {
            var patientExists = await _context.Patients
                .AnyAsync(patient => patient.Id == patientId, cancellationToken);

            if (!patientExists)
            {
                return new(CreateMedicationOrderStatus.PatientNotFound, Message: $"Patient with ID {patientId} was not found.");
            }

            var medicationIds = request.Items.Select(item => item.MedicationId).ToArray();
            var medications = await _context.Medications
                .Where(medication => medication.PatientId == patientId && medicationIds.Contains(medication.Id))
                .ToDictionaryAsync(medication => medication.Id, cancellationToken);

            if (medications.Count != medicationIds.Length)
            {
                return new(CreateMedicationOrderStatus.MedicationNotFound, Message: "One or more medications were not found for this patient.");
            }

            var inactiveMedication = medications.Values.FirstOrDefault(medication => !medication.IsActive);
            if (inactiveMedication is not null)
            {
                return new(CreateMedicationOrderStatus.MedicationInactive, Message: $"Medication '{inactiveMedication.Name}' is inactive.");
            }

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

            var order = new MedicationOrder
            {
                PatientId = patientId,
                OrderedAt = DateTime.UtcNow
            };

            foreach (var requestedItem in request.Items)
            {
                var medication = medications[requestedItem.MedicationId];
                var lineTotal = medication.UnitPrice * requestedItem.Quantity;

                order.Items.Add(new MedicationOrderItem
                {
                    MedicationId = medication.Id,
                    Quantity = requestedItem.Quantity,
                    UnitPrice = medication.UnitPrice,
                    LineTotal = lineTotal
                });

                order.TotalAmount += lineTotal;
                medication.StockQuantity -= requestedItem.Quantity;
            }

            _context.MedicationOrders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            var responseItems = order.Items
                .Select(item => new MedicationOrderItemResponse(
                    item.MedicationId,
                    medications[item.MedicationId].Name,
                    item.Quantity,
                    item.UnitPrice,
                    item.LineTotal))
                .ToArray();

            return new(
                CreateMedicationOrderStatus.Created,
                new MedicationOrderResponse(order.Id, order.PatientId, order.OrderedAt, order.TotalAmount, responseItems));
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
