namespace CardiacMonitor.Models
{
    public class Medication
    {

        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // Nullable في حال كان العلاج مستمراً
        public bool IsActive { get; set; }
    }
}
