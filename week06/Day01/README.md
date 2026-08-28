# Week 6 - Day 1: Sprint 1 Planning & Project Database Design

## Day Overview

Day 1 focused on running our first official Sprint Planning session, defining the "Definition of Done" for Sprint 1, mapping out the entire 3NF-normalized database schema for the capstone project in one pass, and breaking down the sprint scope into realistically sized backlog tasks [3, 4].

## What We Learned

- How to conduct a Sprint Planning session and define "Done" as a secure, functional database schema and core routes [3].
- Why designing the entire database schema in one pass prevents relational anomalies and refactoring friction in later sprints [3].
- Applying First, Second, and Third Normal Form (1NF, 2NF, 3NF) principles to clinical data structures [4].
- Sizing monolithic tasks into granular, half-day to full-day backlog items to keep daily stand-up meetings meaningful [4].

## Tasks We Completed

### Task 1: Sprint 1 Goal & Backlog Board Setup

We established a clear, one-sentence goal at the top of our backlog and configured our board columns (Notion/Trello) to track progress [3, 4].

- **Sprint 1 Goal:** *"Establish a normalized, secure database schema with Entity Framework Core and build paginated retrieval endpoints for patients alongside a transaction-backed appointment scheduling pipeline."* [3, 7]
- **Board Columns:** `Backlog` ➔ `To Do` ➔ `In Progress` ➔ `PR/Review` ➔ `Done` [4].

### Task 2: Full Entity Baseline List

We identified the complete set of entities required across the professional baseline of our Healthcare Management API [3]:

1. `IdentityUser` (AspNetUsers) - Credentials & Login.
2. `IdentityRole` (AspNetRoles) - System Roles (`Admin`, `Doctor`, `Patient`).
3. `Patient` - Demographic Profiles.
4. `VitalSign` - Telemetry Readings.
5. `Medication` - Prescription Plans.
6. `Appointment` - Consultation Schedules.
7. `RefreshToken` - Session Security.

### Task 3: Normalized Schema Design (3NF)

We designed our tables to satisfy all database normalization rules to prevent data duplication and anomalies [4]:

* **1NF:** Atomic attributes in every column (e.g., separating `FirstName` and `LastName`, ensuring no comma-separated fields) [4].
* **2NF:** No partial dependencies; all non-key attributes depend entirely on the primary key (`Id`) [4].
* **3NF:** No transitive dependencies; doctor credentials reside strictly in `AspNetUsers`, and patient demographics remain isolated in the `Patients` table, keeping relationships decoupled and clean [4].

### Task 4: Finalized ERD Diagram

We documented the schema as an ERD with explicit cardinalities and delete constraints to prevent circular cascades [3, 4].
+------------------+ 1 : 1 +------------------+
| AspNetUsers | <---------------------> | Patients |
+------------------+ +------------------+
| PK Id | | PK Id |
| Email | | FK UserId |
+------------------+ | FirstName |
| +------------------+
| 1 |
| | 1
| |
| +-------------+-------------+
| | 1 : N | 1 : N | 1 : N
| ▼ ▼ ▼
| 1 : N +------------+ +-------------+ +------------+
| | VitalSigns | | Medications | |Appointments|
| +------------+ +-------------+ +------------+
| | PK Id | | PK Id | | PK Id |
| | FK PatientId | | FK PatientId | | FK PatientId|
| | HeartRate | | Name | | FK DoctorId|
+------------------------> | OxygenSat | | IsActive| | AppDate |
DoctorId (FK) +------------+ +-------------+ +------------+
code
Code
### Task 5: Backlog Task Sizing

We broke down Sprint 1's scope into estimable, sized backlog tasks to maintain daily transparency during stand-ups [4].

| Task ID | Task Description | Estimated Effort | Status |
| :--- | :--- | :---: | :--- |
| **TSK-101** | Implement `Patients` retrieval endpoint with pagination (`Skip` & `Take`) [6]. | 0.5 Day | To Do |
| **TSK-102** | Add optional search and filtering query parameters (Gender, Name) to Patients [6]. | 0.5 Day | To Do |
| **TSK-103** | Implement dynamic sorting (by `LastName` or `DateOfBirth` with ASC/DESC) [7]. | 0.5 Day | To Do |
| **TSK-104** | Implement business logic check for overlapping doctor appointments [7]. | 0.5 Day | To Do |
| **TSK-105** | Wrap appointment creation in an EF Core Database Transaction (`BeginTransactionAsync`) [7]. | 0.5 Day | To Do |
| **TSK-106** | Write integration tests using `WebApplicationFactory` for paginated and transactional endpoints [6, 7]. | 1.0 Day | To Do |

## Files/Artifacts Related to Day 1

- `docs/ERD_Diagram.png` (Finalized ERD Image) [3, 4]
- `Notion Project Backlog` (Sprint 1 Board Link) [4]

## Day Result

Sprint 1 planning is fully finalized with a locked backlog and an audited, 3NF-compliant ERD schema [3, 4]. This provides a clear, estimable, and secure database blueprint before any C# code is written [3].