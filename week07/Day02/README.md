<div align="center">
  <h1>🔐 Week 7 - Day 2: JWT Login & Registration</h1>
</div>

---

## 📌 Day Overview

Day 2 focused on establishing a secure profiling pipeline for our capstone project [1]. We linked our domain-level `Patient` entity directly to the `IdentityUser`, implemented a registration endpoint that creates both records together within a single database transaction, and configured our login endpoint to issue JWTs containing a domain-specific `PatientId` claim to optimize downstream authorization [5].

## ✅ What We Learned

- Why separating authentication data (`IdentityUser`) from clinical domain data (`Patient`) is an industry-standard best practice [4].
- How to design transaction boundaries around registration endpoints to prevent incomplete sign-ups where a user account is created without an associated medical profile [5].
- The performance benefit of embedding domain-relevant claims (like `PatientId`) directly into the JWT, eliminating extra database queries on every authenticated request [5].
- Simulating and verifying end-to-end registration-to-login flows using Postman [5].

## 🧩 Tasks We Completed

### Task 1: Add a Foreign Key Linking the Patient Entity to its IdentityUser

We added a foreign key property (`UserId`) to our `Patient` entity to link it cleanly with ASP.NET Core's built-in `IdentityUser` table [4, 5].

```csharp
namespace CardiacMonitor.Models;

public class Patient
{
    public int Id { get; set; }
  
    // Foreign Key linking this Patient profile to their IdentityUser
    public string? UserId { get; set; } 
  
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
  
    public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
}
Task 2: Implement a Registration Endpoint with Transaction Protection
We implemented a secure registration flow. If a user registers with the Patient role, the system creates the IdentityUser and the corresponding Patient profile together [5]. Both creations are wrapped in a single database transaction (BeginTransactionAsync) to ensure an all-or-nothing behavior [5, 7].
code
C#
// Inside Services/AuthService.cs
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    var user = new IdentityUser { UserName = request.Email, Email = request.Email };

    // 1. Create the security IdentityUser
    var result = await _userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
    {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return new AuthResponse(false, $"Registration failed: {errors}");
    }

    // 2. Assign the planned domain role
    await _userManager.AddToRoleAsync(user, request.Role);

    // 3. Create the corresponding Patient clinical profile
    if (request.Role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
    {
        var patient = new Patient
        {
            UserId = user.Id,
            FirstName = request.FirstName ?? "New",
            LastName = request.LastName ?? "Patient",
            DateOfBirth = request.DateOfBirth ?? DateTime.UtcNow.AddYears(-30),
            Gender = request.Gender ?? "Unknown",
            ContactNumber = request.ContactNumber ?? string.Empty
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
    }

    // Commit both operations together
    await transaction.CommitAsync();
    return new AuthResponse(true, "User and clinical profile registered successfully.");
}
catch (Exception ex)
{
    // Rollback completely on any failure
    await transaction.RollbackAsync();
    return new AuthResponse(false, $"Registration failed: {ex.Message}");
}
Task 3: Issue JWTs with Domain-Relevant Claims (PatientId)
We configured our token generator to automatically retrieve the linked PatientId from our database when a patient logs in and embed it as a custom claim ("PatientId") inside the JWT [5].
code
C#
// Inside Services/AuthService.cs - GenerateTokenPairAsync method
var authClaims = new List<Claim>
{
    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim(ClaimTypes.Email, user.Email!),
    new Claim(ClaimTypes.NameIdentifier, user.Id)
};

// Add standard user roles as claims
foreach (var role in userRoles)
{
    authClaims.Add(new Claim(ClaimTypes.Role, role));
}

// If the user is in the Patient role, fetch their PatientId and add it as a custom claim
if (userRoles.Contains("Patient"))
{
    var patient = await _context.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id);
    if (patient != null)
    {
        authClaims.Add(new Claim("PatientId", patient.Id.ToString()));
    }
}
Task 4 & 5: Postman Verification & Git Feature Branch Commitment
We fully tested the registration-to-login flow end-to-end and committed our work to our dedicated Git feature branch [5, 6].
Register: Sent a POST request to /api/auth/register with patient details [2].
Database Check: Confirmed in SQL Server that both AspNetUsers and Patients tables received new, linked records.
Login: Sent a POST request to /api/auth/login [2].
JWT Decode: Decoded the returned token on jwt.io and successfully verified that the custom claim "PatientId" is present with the correct ID [2].
Git Commitment: Saved and pushed all changes to feature/sprint2-identity-integration [5].
Files Related to Day 2
Models/Patient.cs [5]
DTOs/AuthDtos.cs
Services/IAuthService.cs, Services/AuthService.cs [2, 5]
Controllers/AuthController.cs [2]
Day Result
Registration and login pipelines are successfully aligned with our domain-level needs [5]. The database transaction prevents orphaned user accounts, and the embedded PatientId token claim enables secure, stateless ownership verification without making redundant database lookups [5].
```
