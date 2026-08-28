# Week 6 - Day 3: Implementing Core Routes I: Catalog & Read Operations

## Day Overview

Day 3 focused on implementing a high-performance, paginated catalog retrieval endpoint for our primary resource (`Patients`) instead of returning entire database tables at once [6]. We integrated optional query parameters to support flexible filtering and dynamic sorting, projecting our database queries directly to safe, dedicated response DTOs to prevent over-fetching and avoid circular reference serialization crashes [6].

## What We Learned

- Why unpaginated "list everything" endpoints break at production scale, causing server-side memory bloat and API timeouts [6].
- Translating LINQ's `.Skip()` and `.Take()` methods into optimized, database-level SQL offsets [6].
- Implementing dynamic, conditional filtering using C#’s `IQueryable` and Deferred Execution [6].
- Decoupling the public API contract from the internal database schema by projecting queries directly to DTOs via `.Select()` [6].
- Mitigating the N+1 query problem and preventing over-fetching to save database I/O, network bandwidth, and web server RAM allocation [6].

## Tasks We Completed

### Task 1: Implement a Paginated GET Endpoint

We established database-level offset pagination on our primary `Patients` catalog query using LINQ’s `.Skip()` and `.Take()` methods, returning a structured paginated list alongside metadata [6].

```csharp
var patients = await query
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(p => new PatientResponse(p.Id, p.UserId, p.FirstName, p.LastName, p.DateOfBirth, p.Gender, p.ContactNumber))
    .ToListAsync();

var totalCount = await query.CountAsync();
return new PaginatedList<PatientResponse>(patients, totalCount, pageNumber, pageSize);
Task 2: Add Optional Filter Query Parameters
We added conditional query parameters to allow clients to filter the patient catalog by searchName (searching first and last name) or gender dynamically without hitting separate endpoints [6].
code
C#
if (!string.IsNullOrWhiteSpace(searchName))
{
    query = query.Where(p => p.FirstName.Contains(searchName) || p.LastName.Contains(searchName));
}

if (!string.IsNullOrWhiteSpace(gender))
{
    query = query.Where(p => p.Gender == gender);
}
Task 3: Add Dynamic Sorting Support
We configured sorting options utilizing a C# switch expression, enabling clients to sort patients dynamically by LastName or DateOfBirth in ascending or descending order [7].
code
C#
query = sortBy.ToLower() switch
{
    "lastname" => isDescending ? query.OrderByDescending(p => p.LastName) : query.OrderBy(p => p.LastName),
    "dateofbirth" => isDescending ? query.OrderByDescending(p => p.DateOfBirth) : query.OrderBy(p => p.DateOfBirth),
    _ => isDescending ? query.OrderByDescending(p => p.FirstName) : query.OrderBy(p => p.FirstName)
};
Task 4: Project Queries Directly to Response DTOs
Rather than exposing database entities directly, we projected the database-level query using .Select() directly to a dedicated PatientResponse DTO, preventing over-fetching and protecting internal DB columns [6, 7].
code
C#
// The Select projection ensures SQL Server only retrieves specified columns:
// SELECT [Id], [UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [ContactNumber] FROM [Patients]
.Select(p => new PatientResponse(p.Id, p.UserId, p.FirstName, p.LastName, p.DateOfBirth, p.Gender, p.ContactNumber))
Task 5: Validate and Test via Postman
We verified all catalog endpoint variations in Postman, ensuring pagination, filtering, and sorting parameters compile into correct SQL queries and return standard HTTP 200 OK responses with matching metadata [7].
code
JSON
{
  "items": [
    {
      "id": 1,
      "userId": null,
      "firstName": "Ahmad",
      "lastName": "Amr",
      "dateOfBirth": "1990-05-12T00:00:00",
      "gender": "Male",
      "contactNumber": "+9759835279"
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 1
}
Files Related to Day 3
DTOs/PatientDtos.cs [6]
Services/IPatientService.cs, Services/PatientService.cs [6]
Controllers/PatientsController.cs [6]
Day Result
The primary patient catalog endpoint is now optimized for scalable read operations [6]. By leveraging deferred database-level execution, offset pagination, dynamic filters, and targeted DTO projections, we prevent over-fetching and secure the public API contract [6].