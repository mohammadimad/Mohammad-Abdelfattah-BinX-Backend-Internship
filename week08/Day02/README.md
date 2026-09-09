# Week 8 - Day 2: Query Optimization with Eager & Explicit Loading

## Day Overview

**Day 2 transitioned from "Diagnosis" to "Treatment." Following the N+1 bottleneck identified on Day 1, we implemented high-performance querying strategies using** **Eager Loading**, **Projection**, and **Split Queries**. These optimizations significantly reduced database round-trips and memory overhead, ensuring the Cardiac Monitor API can scale efficiently.

## What We Learned

* **Eager Loading (.Include):** **How to use JOINs to fetch related data in a single SQL command, effectively eliminating the N+1 problem.**
* **Projection (.Select):** **Why selecting specific columns is the ultimate performance win, reducing I/O by preventing "Over-fetching" of unnecessary data.**
* **Split Queries (.AsSplitQuery):** **Navigating the trade-off between a single massive JOIN (Cartesian product risk) and multiple targeted queries for complex data structures.**
* **Deferred Execution:** **Leveraging** **IQueryable** **to build complex filters and projections that execute entirely on the Database Server.**

## Tasks & Implementation Evidence

### Task 1: Fix N+1 using Eager Loading (**Include**)

**We refactored the retrieval logic to use** **.Include()**, forcing EF Core to perform a SQL JOIN. This collapsed the previous 56 queries into **one single optimized query**.

**Architectural Pattern:**
[GET] -> [/api/performance/fixed-performance] -> [Authorize] -> [Async LINQ with .Include(p => p.VitalSigns) and .AsNoTracking()]

**code**C#

```
// Inside Services/PatientService.cs
public async Task<IEnumerable<PatientVitalsSummaryDto>> GetFixedPerformanceAsync()
{
    var patients = await _context.Patients
        .Include(p => p.VitalSigns) // Resolves N+1 via SQL JOIN
        .AsNoTracking()
        .Take(10)
        .ToListAsync();

    return patients.Select(p => new PatientVitalsSummaryDto(p.FirstName, p.VitalSigns.Count));
}
```

---

### Task 2: Optimized Projection as an alternative to Include

**For list-style endpoints, we implemented** **Projection**. This tells SQL Server to only read the required columns (e.g., Name and HeartRate), avoiding the heavy load of full entity tracking.

**Architectural Pattern:**
[GET] -> [/api/performance/optimized-projection] -> [Authorize] -> [Async LINQ .Select() projecting directly to DTO]

**code**C#

```
// Inside Services/PatientService.cs
public async Task<IEnumerable<PatientProjectionDto>> GetPatientsWithProjectionAsync()
{
    return await _context.Patients
        .AsNoTracking()
        .Select(p => new PatientProjectionDto(
            p.FirstName + " " + p.LastName,
            p.VitalSigns.OrderByDescending(v => v.RecordedAt).Select(v => (int?)v.HeartRate).FirstOrDefault()
        )) // SQL now only retrieves 2 columns instead of the entire row
        .ToListAsync();
}
```

---

### Task 3: Handle Multiple Collections with Split Queries

**When fetching multiple related collections (VitalSigns and MedicalAlerts), a single JOIN would create a Cartesian product explosion. We applied** **.AsSplitQuery()** **to maintain linear performance.**

**code**C#

```
// Inside Services/PatientService.cs
public async Task<PatientFullDashboardDto?> GetFullDashboardAsync(int id)
{
    var patientData = await _context.Patients
        .Include(p => p.VitalSigns)
        .Include(p => p.MedicalAlerts)
        .AsSplitQuery() // 🚀 Optimizes performance for multiple 1-to-Many relations
        .FirstOrDefaultAsync(p => p.Id == id);
  
    // Mapping logic...
}
```

---

### Task 4 & 5: Measured Performance Improvement (The Results)

**We re-ran our diagnostics with logging enabled. The results prove a massive reduction in database pressure:**


| **Endpoint Scenario**      | **Day 1 (Baseline)** | **Day 2 (Optimized)** | **SQL Technique**       | **Result**  |
| -------------------------- | -------------------- | --------------------- | ----------------------- | ----------- |
| **List Patients + Vitals** | **56 Queries**       | **1 Query**           | **Eager Loading**       | **Pass ✅** |
| **Summarized List**        | **56 Queries**       | **1 Query**           | **Projection (Select)** | **Pass ✅** |
| **Full Patient Dashboard** | **N/A**              | **3 Queries**         | **Split Query**         | **Pass ✅** |

## Files Related to Day 2

* **Services/IPatientService.cs** **(Updated Interface)**
* **Services/PatientService.cs** **(Optimization Logic)**
* **Controllers/PerformanceController.cs** **(Clean "Thin" Controller)**
* **DTOs/PerformanceDtos.cs** **(Data Transfer Objects for Reports)**

## Day Result

**The API is now "Production-Hardened." By moving the logic from the controller to the service layer and applying advanced LINQ patterns, we reduced database round-trips by** **98%** **for our primary list endpoints while maintaining a clean, maintainable architecture.**
