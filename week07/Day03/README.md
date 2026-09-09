# Week 7 - Day 3: Role-Based Access Control & Ownership Checks

## Day Overview

Day 3 focused on implementing a robust, dual-layered authorization architecture on top of our authentication model [6]. We configured strict Role-Based Access Control (RBAC) across our three domain roles (`Admin`, `Doctor`, `Patient`), applied exact route policies, and implemented stateless, claim-backed ownership checks to eliminate Insecure Direct Object Reference (IDOR/BOLA) vulnerabilities [6, 7].

## What We Learned

- Why role checks alone only verify *what* kind of user is calling, but fail to verify *which* specific records they are allowed to access (IDOR vulnerability) [6, 7].
- Implementing stateless resource-based authorization by extracting custom claims (`PatientId`) directly from the JWT to verify data ownership [5, 6].
- Why negative testing (verifying that unauthorized tokens are rejected with HTTP 403 Forbidden) is the only real proof that an authorization model is secure [6].

## Tasks We Completed

### Task 1: Seed Initial Admin Account & Default Role Assignment

We ensured new public registrations are assigned the `Patient` role by default [6], while seeding an administrative account directly via `OnModelCreating` to prevent unauthorized role escalation [4, 7].

```csharp
// Inside Data/AppDbContext.cs - Seeding a secure, initial Admin account
var adminUserId = "admin-id-999";
var adminUser = new IdentityUser
{
    Id = adminUserId,
    UserName = "admin@cardiac.com",
    NormalizedUserName = "ADMIN@CARDIAC.COM",
    Email = "admin@cardiac.com",
    NormalizedEmail = "ADMIN@CARDIAC.COM",
    EmailConfirmed = true,
    SecurityStamp = Guid.NewGuid().ToString()
};
adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

builder.Entity<IdentityUser>().HasData(adminUser);

// Link Admin user to the Admin role
builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
{
    RoleId = adminRoleId,
    UserId = adminUserId
});
```

### Task 2: Configure Endpoint Access Levels (RBAC Mapping)

We audited all controllers and applied explicit **[Authorize]** and role requirements to guarantee secure access boundaries [6, 7].

```csharp
// 1. Public Endpoint - No Authentication
[HttpGet("api/patients")] // Overridden inside, but let's review general actions
[AllowAnonymous]

// 2. High-Privilege Endpoint - Admin Only
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Create([FromBody] CreatePatientRequest request) { ... }

// 3. Clinical Endpoint - Admin and Doctor Only
[HttpPut("{id}")]
[Authorize(Roles = "Admin,Doctor")]
public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientRequest request) { ... }
```

### Task 3: Stateless Ownership Checks (IDOR/BOLA Protection)

We utilized our custom **"PatientId"** claim (added to the JWT during login) to perform extremely fast, stateless ownership checks [5, 6]. If a user with the **Patient** role attempts to query clinical data belonging to another patient, they are immediately blocked [6, 7].

```csharp
// Inside Controllers/PatientsController.cs - GetById action
[HttpGet("{id}")]
[Authorize]
public async Task<IActionResult> GetById(int id)
{
    var patient = await _patientService.GetPatientByIdAsync(id);
    if (patient == null)
    {
        return NotFound(new { Message = $"Patient with ID {id} was not found." });
    }

    // 1. Read claims from the validated JWT token
    var isPatient = User.IsInRole("Patient");
    var loggedInPatientId = User.FindFirst("PatientId")?.Value;

    // 2. Enforce stateless ownership check
    if (isPatient && patient.Id.ToString() != loggedInPatientId)
    {
        return Forbid(); // Returns HTTP 403 Forbidden statelessly!
    }

    return Ok(patient);
}
```

---

### Task 4 & 5: Negative Security Testing & Rejection Scenarios

We designed and executed specific Postman tests to verify that unauthorized requests are successfully rejected with HTTP 403 Forbidden [6, 7].

- **Test 1 (Role Block):** Logged in as a **Patient** and attempted to call an Admin-only route (**POST /api/patients**) [6, 7].

  - **Result:** Rejected with **HTTP 403 Forbidden** [6].

- **Test 2 (IDOR Block):** Logged in as Patient A (holding claim **PatientId = 1**) and attempted to query Patient B's vitals (**GET /api/patients/2/vitals**) [6, 7].

  - **Result:** The controller extracted **PatientId = 1** from the token, compared it with the path parameter **patientId = 2**, detected the mismatch, and rejected the request with **HTTP 403 Forbidden** [6, 7].

## Files Related to Day 3

- **Controllers/PatientsController.cs** (Secured with RBAC and stateless ownership checks) [6]
- **Controllers/VitalSignsController.cs**, **Controllers/MedicationsController.cs**, **Controllers/AppointmentsController.cs** (Secured with RBAC and ownership checks) [6]
- **Data/AppDbContext.cs** (Seeded initial Admin account) [4, 7]

## Day Result

**The API is now robustly protected against direct object references and role escalation [6, 7]. By combining strict endpoint-level RBAC with stateless, claim-backed ownership checks, we guarantee that clinical and demographic records can only be accessed by authorized professionals or the patients themselves [5, 6, 7].**
