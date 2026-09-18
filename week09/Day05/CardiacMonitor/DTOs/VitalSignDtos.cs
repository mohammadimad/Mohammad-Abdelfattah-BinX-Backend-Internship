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

        public sealed record VitalSignQueryParameters(
            int Page = 1,
            int PageSize = 20,
            DateTime? From = null,
            DateTime? To = null,
            int? MinHeartRate = null,
            int? MaxHeartRate = null,
            string Sort = "recordedAt_desc"
        );

        public sealed record PagedResult<T>(
            IReadOnlyList<T> Items,
            int Page,
            int PageSize,
            int TotalCount,
            int TotalPages
        );
}
