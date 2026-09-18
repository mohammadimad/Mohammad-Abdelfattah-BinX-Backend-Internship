namespace CardiacMonitor.DTOs
{
    

        public record CreateAppointmentRequest(
            string DoctorId,
            DateTime AppointmentDate,
            string Status,
            string? Notes
        );

        public record UpdateAppointmentRequest(
            string DoctorId,
            DateTime AppointmentDate,
            string Status,
            string? Notes
        );

        public record AppointmentResponse(
            int Id,
            int PatientId,
            string DoctorId,
            DateTime AppointmentDate,
            string Status,
            string? Notes
        );

}
