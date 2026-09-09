namespace CardiacMonitor.DTOs
{
    public record PatientVitalsSummaryDto(string PatientName, int VitalsCount);

    public record PatientFullDashboardDto(string FirstName, int VitalsCount, int AlertsCount);

    public record PatientProjectionDto(string PatientName, int? LatestHeartRate);
}

