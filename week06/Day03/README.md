# Week 6 - Day 3

## Implementing Core Routes I: Catalog & Read Operations

> Building a scalable, filtered, and paginated patient catalog API.

## Overview

On Day 3, we focused on implementing a high-performance catalog endpoint for the `Patients` resource instead of returning the full database table in one response. The goal was to improve scalability, reduce payload size, and maintain a clean public API contract.

We added optional query parameters for filtering and sorting, and we projected the database results directly into dedicated response DTOs to avoid over-fetching and prevent circular reference serialization issues.

---

## Learning Outcomes

During this task, we learned how to:

- Avoid unpaginated list endpoints that can cause memory bloat and request timeouts.
- Use LINQ's `.Skip()` and `.Take()` to implement efficient database-level pagination.
- Apply dynamic filtering using `IQueryable` and deferred execution.
- Separate the API contract from the internal database model using DTO projections.
- Reduce unnecessary data transfer and improve database efficiency.

---

## Tasks Completed

### 1. Paginated GET Endpoint

We implemented a paginated catalog query that returns only the required records, along with metadata such as total count, current page, and page size.

```csharp
var patients = await query
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(p => new PatientResponse(
        p.Id,
        p.UserId,
        p.FirstName,
        p.LastName,
        p.DateOfBirth,
        p.Gender,
        p.ContactNumber))
    .ToListAsync();

var totalCount = await query.CountAsync();
return new PaginatedList<PatientResponse>(patients, totalCount, pageNumber, pageSize);
```

### 2. Optional Filter Query Parameters

Clients can now filter the catalog by name or gender without creating separate endpoints.

```csharp
if (!string.IsNullOrWhiteSpace(searchName))
{
    query = query.Where(p =>
        p.FirstName.Contains(searchName) || p.LastName.Contains(searchName));
}

if (!string.IsNullOrWhiteSpace(gender))
{
    query = query.Where(p => p.Gender == gender);
}
```

### 3. Dynamic Sorting Support

We added sorting support for both ascending and descending order, based on configurable query values.

```csharp
query = sortBy.ToLower() switch
{
    "lastname" => isDescending
        ? query.OrderByDescending(p => p.LastName)
        : query.OrderBy(p => p.LastName),

    "dateofbirth" => isDescending
        ? query.OrderByDescending(p => p.DateOfBirth)
        : query.OrderBy(p => p.DateOfBirth),

    _ => isDescending
        ? query.OrderByDescending(p => p.FirstName)
        : query.OrderBy(p => p.FirstName)
};
```

### 4. DTO Projection for Safe Read Responses

Instead of returning database entities directly, the query is projected to `PatientResponse`, which keeps the API contract controlled and prevents exposing internal database fields.

```csharp
.Select(p => new PatientResponse(
    p.Id,
    p.UserId,
    p.FirstName,
    p.LastName,
    p.DateOfBirth,
    p.Gender,
    p.ContactNumber))
```

### 5. Validation with Postman

We tested the endpoint through Postman to confirm that pagination, filtering, and sorting behave correctly and return valid HTTP 200 responses.

```json
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
```

---

## Related Files

- `DTOs/PatientDtos.cs`
- `Services/IPatientService.cs`
- `Services/PatientService.cs`
- `Controllers/PatientsController.cs`

---

## Final Result

The patient catalog endpoint is now optimized for scalable read operations. By combining deferred execution, offset pagination, dynamic filters, and DTO projections, we reduced over-fetching and improved the reliability and maintainability of the API.

> This implementation creates a cleaner and more production-ready read flow while protecting internal entity data and reducing the load on the server.

