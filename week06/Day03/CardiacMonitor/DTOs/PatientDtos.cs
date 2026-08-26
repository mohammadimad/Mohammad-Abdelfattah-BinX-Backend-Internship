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
    // Generic capsule object for transferring page data (Paginated List DTO)
    public record PaginatedList<T>(
            IEnumerable<T> Items,
            int TotalCount,
            int PageNumber,
            int PageSize
    );
}
