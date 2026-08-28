namespace CardiacMonitor.Models;

public class MedicationOrderItem
{
    public int Id { get; set; }
    public int MedicationOrderId { get; set; }
    public MedicationOrder MedicationOrder { get; set; } = null!;
    public int MedicationId { get; set; }
    public Medication Medication { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
