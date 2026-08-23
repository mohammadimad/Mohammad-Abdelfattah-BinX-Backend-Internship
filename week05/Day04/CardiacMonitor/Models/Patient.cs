namespace CardiacMonitor.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string? UserId { get; set; } = null;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;

        public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
    }
}
