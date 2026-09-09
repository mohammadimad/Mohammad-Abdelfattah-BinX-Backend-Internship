```

# Week 7 - Day 1: Sprint 2 Planning & Wiring Identity

## Day Overview
Day 1 marked the kickoff of Sprint 2, focusing on planning our authorization model and integrating ASP.NET Core Identity into our existing database schema [1, 3]. We successfully upgraded our capstone's `AppDbContext` to inherit from `IdentityDbContext`, ran migrations to apply the Identity schema, defined our domain-specific roles (`Admin`, `Doctor`, `Patient`), and established a solid Role-Based Access Control (RBAC) matrix [3, 4].

## What We Learned
- How to transition an existing DbContext with active seed data and migrations to a secure `IdentityDbContext` [4, 5].
- Managing database migrations carefully when retrofitting identity frameworks onto pre-existing database tables [4, 5].
- Designing clinical domain roles (`Admin`, `Doctor`, `Patient`) rather than improvising checks endpoint by endpoint [3, 6].
- Translating retrospective improvements (e.g., writing Postman tests alongside endpoints) into actionable backlog tasks [6, 9].

## Tasks We Completed

### Task 1: Sprint 2 Planning & Sized Backlog (with Sprint 1 Retro Action)
We formulated our Sprint 2 goal and created a sized backlog in our project tracker [3, 4]. In line with our Sprint 1 retrospective improvement, **we have explicitly required that each Sprint 2 task's Postman tests be written and configured first before the endpoint card is moved to Done** [6, 9].

*   **Sprint 2 Goal:** *"Secure the capstone API by integrating ASP.NET Core Identity, implementing JWT-based login with dynamic PatientId claims, and enforcing role-based and resource-based (ownership) authorization across all endpoints."* [3, 5, 7]

#### Sprint 2 Sized Backlog Table:
| Task ID | Task Description / Definition of Done | Sizing | Priority |
| :--- | :--- | :---: | :---: |
| **TSK-201** | Upgrade `AppDbContext` to `IdentityDbContext` and apply Identity tables via migrations [4]. | 0.5 Day | P0 |
| **TSK-202** | Implement transaction-backed `/register` (creating user & patient profile) and JWT login [5]. | 1.0 Day | P0 |
| **TSK-203** | Apply global role-based authorization and resource-based ownership checks to endpoints [6, 7]. | 1.0 Day | P0 |
| **TSK-204** | Implement custom `RequestTimingMiddleware` to measure and log HTTP pipeline duration [8]. | 0.5 Day | P1 |
| **TSK-205** | Write automated Postman test examples (Happy/Error paths) for all Sprint 2 protected routes [9]. | 1.0 Day | P0 |

---

### Task 2: Change Capstone DbContext to Inherit from IdentityDbContext
We refactored `AppDbContext` to inherit from `IdentityDbContext<IdentityUser>` instead of `DbContext` to seamlessly wire ASP.NET Core Identity tables alongside our existing domain tables [3, 4].

```csharp
using CardiacMonitor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Required namespace
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitor.Data;

public class AppDbContext : IdentityDbContext<IdentityUser> // Changed inheritance here
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Critical for loading Identity tables
      
        // Explicit Fluent API relationship configurations...
    }
}
```

---

### Task 3: Generate, Review, and Apply Identity Migration

**We generated a code-first migration to apply the ASP.NET Core Identity schema (Users, Roles, UserRoles, Claims tables) without destroying existing domain database tables [4, 5].**

**code**Bash

```
# 1. Generate the migration to retrofit Identity tables
dotnet ef migrations add AddIdentitySchema -o Data/Migrations

# 2. Review the generated migration file for schema accuracy and safety (verified zero data-loss warnings)

# 3. Apply the migration to the local SQL Server database
dotnet ef database update
```

---

### Task 4 & 5: Domain Roles Planning & Authorization Matrix (RBAC)

**We defined three essential system roles mapped directly to our healthcare system domain needs [3, 4]:**

* **Admin:** **Administrative scope (manages core patient records, user registration, and platform audits).**
* **Doctor:** **Clinical scope (views profiles, manages patient vital signs, medications, and schedules appointments).**
* **Patient:** **Restricted self-service scope (can only read their own personal medical profile and clinical records).**

#### Sprint 2 Role-Based Access Control (RBAC) Matrix:


| **API Endpoint**                           | **HTTP Method** | **Access Level** | **Required Role**        | **Ownership Check? (IDOR Protection)**                     |
| ------------------------------------------ | --------------- | ---------------- | ------------------------ | ---------------------------------------------------------- |
| **/api/auth/register**                     | **POST**        | **Public**       | **Anonymous**            | **No**                                                     |
| **/api/auth/login**                        | **POST**        | **Public**       | **Anonymous**            | **No**                                                     |
| **/api/patients**                          | **GET**         | **Protected**    | **Admin, Doctor**        | **No (Admin/Doctor can view all patients)**                |
| **/api/patients/{id}**                     | **GET**         | **Protected**    | **Any**                  | **Yes** **(Patient can only view their own profile)**      |
| **/api/patients**                          | **POST**        | **Protected**    | **Admin**                | **No (Admin creates the profile)**                         |
| **/api/patients/{patientId}/vitals**       | **GET**         | **Protected**    | **Any**                  | **Yes** **(Patient can only view their own vitals)**       |
| **/api/patients/{patientId}/vitals**       | **POST**        | **Protected**    | **Admin,Doctor,Patient** | **Yes** **(Patient can only insert to their own profile)** |
| **/api/vitals/{id}**                       | **PUT/DELETE**  | **Protected**    | **Admin, Doctor**        | **No**                                                     |
| **/api/patients/{patientId}/medications**  | **GET**         | **Protected**    | **Any**                  | **Yes** **(Patient can only view their own meds)**         |
| **/api/patients/{patientId}/medications**  | **POST**        | **Protected**    | **Admin, Doctor**        | **No**                                                     |
| **/api/patients/{patientId}/appointments** | **GET**         | **Protected**    | **Any**                  | **Yes** **(Patient can only view their own appointments)** |
| **/api/patients/{patientId}/appointments** | **POST**        | **Protected**    | **Admin, Doctor**        | **No**                                                     |

---

## Files Related to Day 1

* **Data/AppDbContext.cs** **(Inherits from** **IdentityDbContext**) [4]
* **Data/Migrations/...\_AddIdentitySchema.cs** **(Identity schema database migration) [4, 5]**

## Day Result

**Sprint 2 planning is locked, the capstone database is migrated with the Identity framework, and a secure, 3NF-compliant RBAC and ownership-check matrix is established before writing any authorization middleware or controller attributes [3, 4].**
