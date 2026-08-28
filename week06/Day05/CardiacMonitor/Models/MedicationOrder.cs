namespace CardiacMonitor.Models;

public class MedicationOrder
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public DateTime OrderedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<MedicationOrderItem> Items { get; set; } = new List<MedicationOrderItem>();
}
