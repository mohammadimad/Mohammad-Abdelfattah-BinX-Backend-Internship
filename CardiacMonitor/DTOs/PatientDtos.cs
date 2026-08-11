namespace CardiacMonitor.DTOs
{
    public class PatientDtos
    {

        public record CreatePatientRequest(
            string FirstName,
            string LastName,
            DateTime DateOfBirth,
            string Gender,
            string ContactNumber
        );

        public record PatientResponse(
            int Id,
            string FirstName,
            string LastName,
            DateTime DateOfBirth,
            string Gender,
            string ContactNumber
        );
    }
}
