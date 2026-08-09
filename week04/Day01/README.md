# 🔐 Day 01: ASP.NET Core Identity & User Registration

## 📝 Overview
This directory documents the integration of **ASP.NET Core Identity** into our Library Management API. Rather than dangerously "reinventing the wheel" by hand-rolling a custom user storage table and hashing mechanism, we leveraged Microsoft's battle-tested membership framework. This establishes a secure, production-grade foundation for user management, password hashing, and role architecture.

---

## 🧠 Core Architectural Concepts Learned

### 1. What ASP.NET Core Identity Provides
ASP.NET Core Identity is a complete, built-in membership system that manages:
- **Authentication:** Verifying user identities securely.
- **Authorization:** Managing roles and permissions.
- **User & Role Stores:** Persisting data seamlessly using Entity Framework Core.
By adopting this industry standard, we avoid severe security vulnerabilities commonly introduced in custom authentication systems.

### 2. Password Hashing Under the Hood (PBKDF2 & Salting)
Password security in ASP.NET Core Identity is managed via rigorous cryptographic standards:
- **PBKDF2 Algorithm:** Uses PBKDF2 (Password-Based Key Derivation Function 2) with HMAC-SHA256. This algorithm is *deliberately slow* (computationally expensive) to dramatically slow down GPU-accelerated brute-force attacks in the event of a database leak.
- **Unique Salting:** A unique, random cryptographic "Salt" is generated per user. This ensures that even if two users share the exact same password, their stored hashes are completely different, neutralizing **Rainbow Table** attacks.

### 3. Database Schema Extension (`IdentityDbContext`)
Integrating Identity extends our relational schema by adding seven core security tables (prefixed with `AspNet`, such as `AspNetUsers` and `AspNetRoles`). This is achieved by changing our DbContext inheritance from the standard `DbContext` to **`IdentityDbContext<IdentityUser>`**.

---

## 🛠️ Technical Implementation

### A. Database Context Configuration
The `LibraryDbContext` was updated to support Identity, ensuring the vital `base.OnModelCreating(builder)` call is preserved:

```csharp
public class LibraryDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // CRUCIAL: Configures composite keys and mapping for Identity internal tables.
        // Forgetting this call causes EF Core migration failures (e.g., identity login key errors).
        base.OnModelCreating(builder); 
    }
}
### B. Service Registration (`Program.cs`)
Identity services were injected into the dependency injection (DI) container and wired up to our EF Core SQL database:

```csharp
builder.Services.AddIdentity()
    .AddEntityFrameworkStores<LibraryDbContext>();
```

### C. Defensive User Registration Endpoint
Implemented the user registration logic in `AuthController` utilizing the `UserManager` service:

* **`CreateAsync`**: Handles both hashing (via PBKDF2) and SQL database persistence in a single unit of work transaction.
* **DTO Protection**: Leveraged `RegisterRequestDto` to accept only required fields (Username, Email, Password), securing the endpoint against Over-Posting attacks.
* **REST Status Codes**: Returns `201 Created` with a Location Header on success, or `400 Bad Request` with structured Identity error arrays upon validation conflicts (e.g., duplicate email or weak password).

---

### 🧪 Hands-On Lab Results
The following operational tasks were completed:

* Installed `Microsoft.AspNetCore.Identity.EntityFrameworkCore` package.
* Executed and applied migrations to generate the `AspNet` infrastructure tables in SQL Server.
* Designed `AuthController` with a secure registration endpoint.
* **Postman Validation**:
  * **Happy Path**: Verified successful registration (Status Code `201 Created`).
  * **Sad Path**: Verified proper validation handling for duplicate emails and weak passwords (Status Code `400 Bad Request` with structured error payloads).

---

### 🧰 Tools Used
* **Framework**: ASP.NET Core Web API (.NET 9)
* **ORM**: Entity Framework Core (Identity Migrations)
* **Database**: SQL Server (LocalDB)
* **Testing**: Postman (Endpoint verification)
