using CardiacMonitor.Models;

namespace CardiacMonitor.Models
{
    public class VitalSign
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int HeartRate { get; set; }
        public decimal OxygenSaturation { get; set; } 
        public int SystolicBP { get; set; }
        public int DiastolicBP { get; set; }
        public DateTime RecordedAt { get; set; }
        public Patient Patient { get; set; } = null!;

    }
}


