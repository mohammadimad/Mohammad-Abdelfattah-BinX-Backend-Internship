namespace CardiacMonitor.DTOs
{
   
        public record CreateMedicationRequest(
            string Name,
            string Dosage,
            string Frequency,
            DateTime StartDate,
            DateTime? EndDate,
            bool IsActive
        );

        public record UpdateMedicationRequest(
            string Name,
            string Dosage,
            string Frequency,
            DateTime StartDate,
            DateTime? EndDate,
            bool IsActive
        );

        public record MedicationResponse(
            int Id,
            int PatientId,
            string Name,
            string Dosage,
            string Frequency,
            DateTime StartDate,
            DateTime? EndDate,
            bool IsActive
        );

}
