namespace CardiacMonitor.DTOs
{
    

        public record CreatePatientRequest(
            string FirstName,
            string LastName,
            DateTime DateOfBirth,
            string Gender,
            string ContactNumber
        );

        public record UpdatePatientRequest(
            string FirstName,
            string LastName,
            DateTime DateOfBirth,
            string Gender,
            string ContactNumber
            );
            public record PatientResponse
            (
           int Id,
           string? UserId, 
           string FirstName,
           string LastName,
           DateTime DateOfBirth,
           string Gender,
           string ContactNumber
          );

        public sealed record PatientQueryParameters(
            int Page = 1,
            int PageSize = 20,
            string? Search = null,
            string? Gender = null,
            string Sort = "firstName_asc"
        );
}
