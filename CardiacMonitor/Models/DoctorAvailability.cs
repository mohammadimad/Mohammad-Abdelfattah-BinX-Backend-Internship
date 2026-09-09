namespace CardiacMonitor.Models;

public class DoctorAvailability
{
    public int Id { get; set; }
    public int DoctorProfileId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;

    public DoctorProfile DoctorProfile { get; set; } = null!;
}
