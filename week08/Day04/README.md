# Week 8 - Day 4: Database Indexing & Performance Profiling

## Day Overview

**Day 4 focused on database engine optimization by identifying critical query bottlenecks, adding targeted composite indexes via EF Core Fluent API, and analyzing SQL Server Execution Plans in SSMS. We converted expensive table scans and sorting operations into direct, single-pass index seeks, producing verifiable before-and-after performance evidence for mentor code review.**

## What We Learned

* **When to Index:** **Targeting columns frequently evaluated in** `WHERE`, `JOIN`, **and** `ORDER BY` **clauses while balancing write overhead.**
* **Composite Indexes:** **Designing multi-column indexes that satisfy both a filter and a sort condition in a single database operation.**
* **Execution Plan Analysis:** **Using SQL Server Management Studio (SSMS) to inspect physical execution operators (**`Clustered Index Scan` **vs.** `Index Seek`**) and cost drivers (e.g.,** `Sort`**).**
* **Empirical Benchmarking via Query Hints:** **Utilizing** `WITH (INDEX(...))` **in SQL to force side-by-side plan comparisons between unindexed and indexed states without rolling back migrations.**

## Tasks & Implementation Evidence

### Task 1: Identify High-Impact Query Columns

**We audited our query patterns across the** **Cardiac Monitor API** **and selected three high-traffic column combinations:**

1. `VitalSigns (PatientId, RecordedAt)`: **Telemetry history constantly filters by patient and sorts chronologically descending.**
2. `Patients (Gender, LastName)`: **The patient catalog regularly filters by demographic criteria and sorts by family name.**
3. `Appointments (DoctorId, AppointmentDate)`: **Schedule queries look up doctor availability, where uniqueness is enforced at the database level.**

---

### Task 2: Configure Indexes via EF Core Fluent API & Migrations

**We configured composite and unique indexes explicitly in** **`AppDbContext.cs`** **using the Fluent API, then generated and applied the migration.**

**Architectural Pattern:**
`Query Pattern` -> **Filter/Sort Columns** -> **EF Core Fluent API** -> **`HasIndex(...).HasDatabaseName(...)`** -> **Migration** -> **Runtime Index Usage**

**code**C#

```csharp
// Inside Data/AppDbContext.cs - OnModelCreating
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // 1. Composite index for chronological patient telemetry history
    builder.Entity<VitalSign>()
        .HasIndex(v => new { v.PatientId, v.RecordedAt })
        .HasDatabaseName("IX_VitalSigns_PatientId_RecordedAt");

    // 2. Composite index for catalog filtering and sorting
    builder.Entity<Patient>()
        .HasIndex(p => new { p.Gender, p.LastName })
        .HasDatabaseName("IX_Patients_Gender_LastName");

    // 3. Composite unique index to avoid overlapping doctor appointments
    builder.Entity<Appointment>()
        .HasIndex(a => new { a.DoctorId, a.AppointmentDate })
        .IsUnique()
        .HasDatabaseName("UX_Appointments_DoctorId_AppointmentDate");
}
```

**code**Bash

```bash
# Generate and apply migration
dotnet ef migrations add AddPerformanceIndexes -o Data/Migrations
dotnet ef database update
```

---

### Task 3 & 4: Profile Execution Plans and Document Improvements

**In SSMS, we enabled** **Include Actual Execution Plan** **(**`Ctrl + M`**) and executed a side-by-side query comparing the baseline state (forced scan via hint) with the newly indexed state.**

**code**SQL

```sql
USE CardiacMonitorDb;
GO

-- 1. Baseline State (Simulating Before Index)
SELECT Id, HeartRate, OxygenSaturation, RecordedAt
FROM VitalSigns WITH (INDEX(PK_VitalSigns))
WHERE PatientId = 5
ORDER BY RecordedAt DESC;

-- 2. Optimized State (After Index)
SELECT Id, HeartRate, OxygenSaturation, RecordedAt
FROM VitalSigns
WHERE PatientId = 5
ORDER BY RecordedAt DESC;
```

#### Measured Performance Improvement Table:

| **Metric**                | **Before Index** (**PK\_VitalSigns** **Scan**) | **After Index** (**IX\_VitalSigns...** **Seek**) | **Measured Improvement** |
| ------------------------- | --------------------------------------------- | ----------------------------------------------- | ----------------------- |
| **Physical Operator**     | **Clustered Index Scan**                       | **Index Seek**                                  | **Direct range traversal without scanning unrelated rows.** |
| **Sort Operator**         | **Present (consumed ~70% of query cost)**      | **Eliminated (0% cost)**                        | **Data is pre-ordered in the index structure.** |
| **Relative Subtree Cost** | **~78% of batch cost**                        | **~22% of batch cost**                          | **~72% reduction in query cost.** |
| **Logical Reads**         | **Scanned all table pages**                    | **3-4 pages read**                              | **Drastic reduction in disk I/O and memory cache churn.** |

---

### Task 5: Branch Management & Pull Request Preparation

**All configuration and migration assets were committed to a dedicated Git branch in preparation for code review:**

**code**Bash

```bash
# Feature branch commit and push
git checkout -b feature/sprint3-database-indexing
git add .
git commit -m "perf: add composite indexes on vitals, patients, and appointments with execution plan evidence"
git push -u origin feature/sprint3-database-indexing
```

## Files Related to Day 4

* **Data/AppDbContext.cs** **(Fluent API index declarations)**
* **Data/Migrations/..._AddPerformanceIndexes.cs** **(Database migration)**

## Day Result

**Database performance optimization is now backed by tangible execution plan evidence. Expensive table scans and in-memory sort operations on vital sign telemetry were replaced with targeted index seeks, decreasing execution cost by ~72% and stabilizing database throughput under load.**
