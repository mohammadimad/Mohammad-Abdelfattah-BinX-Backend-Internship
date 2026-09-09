namespace CardiacMonitor.DTOs
{
           public record CreateVitalSignRequest(
            int HeartRate,
            decimal OxygenSaturation,
            int SystolicBP,
            int DiastolicBP
        );

        public record UpdateVitalSignRequest(
            int HeartRate,
            decimal OxygenSaturation,
            int SystolicBP,
            int DiastolicBP
        );

        public record VitalSignResponse(
            int Id,
            int PatientId,
            int HeartRate,
            decimal OxygenSaturation,
            int SystolicBP,
            int DiastolicBP,
            DateTime RecordedAt
        );
}
