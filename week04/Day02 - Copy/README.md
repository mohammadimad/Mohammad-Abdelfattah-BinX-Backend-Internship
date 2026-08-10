# 📝 Day 1: ASP.NET Core Identity & User Registration

Welcome to Day 1 of Week 4. Today’s focus is on integrating a secure membership system to manage users, secure password storage using built-in hashing algorithms, and build a robust registration endpoint.

## 🎯 Learning Objectives
* Understand the core authentication and membership features provided by ASP.NET Core Identity.
* Configure and bind Identity schema with the database using Entity Framework Core.
* Implement a robust user registration endpoint utilizing `UserManager<TUser>`.
* Understand how PBKDF2 hashing secures stored credentials under the hood.

## 🛠️ Prerequisites & Tools
* **.NET 8.0 SDK** (or later)
* **Postman** for API testing

### Required NuGet Packages:
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## 🚀 Step-by-Step Implementation

### Step 1: Inherit from IdentityDbContext
Modify your existing `DbContext` to inherit from `IdentityDbContext` instead of `DbContext`. This incorporates all necessary Identity tables (Users, Roles, Claims) into your database context.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Add your application's DbSet entities here (e.g., DbSet<Order>)
}
```

### Step 2: Generate and Apply Migrations
Generate a database migration to add the Identity tables and apply it to your database.

```bash
dotnet ef migrations add AddIdentityTables
dotnet ef database update
```

### Step 3: Register Identity Services in Program.cs
Register Identity services to configure password rules and bind them to your database context.

```csharp
// Configure Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity(options =>
{
    // Define Password Complexity Constraints
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
```

### Step 4: Create the Registration Endpoint
Implement a Controller and inject the `UserManager` service to handle user creation and automatic password hashing.

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager _userManager;

    public AuthController(UserManager userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new IdentityUser 
        { 
            UserName = model.Email, 
            Email = model.Email 
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok(new { Message = "User registered successfully." });
        }

        // Return identity errors (e.g., password too weak, email already taken)
        return BadRequest(result.Errors);
    }
}

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
```

---

## 🧪 Testing the Endpoint (Postman)

1. Open **Postman** and create a new **POST** request.
2. Set the URL to: `https://localhost:xxxx/api/auth/register` (replace with your port).
3. Under the **Body** tab, select **raw** and set the format to **JSON**.

### Test Case 1 (Successful Registration):
Send valid data that conforms to the configured password requirements:

```json
{
  "email": "student@binxtech.com",
  "password": "SecurePassword123!"
}
```
* **Expected Response**: `200 OK` with a success message.

### Test Case 2 (Validation Failures):
Send an invalid password (e.g., "123"):

```json
{
  "email": "student@binxtech.com",
  "password": "123"
}
```
* **Expected Response**: `400 Bad Request` containing detailed validation error descriptions.
