# 📚 Day 03: EF Core Setup & Code-First Migrations

## 📝 Overview
This day focuses on integrating **Entity Framework Core 8** into the Library Management API using the **Code-First** approach. Domain entities were modeled to match the normalized (3NF) relational schema, configured via `DbContext`, and deployed to SQL Server through EF Core Migrations.

---

## 🚀 Tech Stack & Packages
- **Framework:** .NET 8 (ASP.NET Core Web API)
- **ORM:** Entity Framework Core 8.0
- **Database:** Microsoft SQL Server
- **NuGet Packages:**
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Tools`

---

## 🛠️ Implementation Steps

### 1️⃣ Domain Models & DbContext
- Mapped C# entities (`Author`, `Book`, `Member`, `MemberPhone`, `LendingRecord`) with 1:N and M:N navigation properties.
- Configured `LibraryDbContext` (or `AppDbContext`) exposing `DbSet<T>` for all domain models.
- Applied Fluent API constraints for monetary precision (`decimal(10,2)`) and unique index constraints (Member Email).

### 2️⃣ Configuration & Security
- Registered `LibraryDbContext` in `Program.cs` with SQL Server provider.
- Managed connection strings securely in `appsettings.Development.json` (excluded from git tracking).

### 3️⃣ Migration & Database Deployment
Executed the Code-First workflow via .NET CLI:

```bash
# 1. Install EF Core CLI Tool (if not installed)
dotnet tool install --global dotnet-ef

# 2. Generate Initial Migration
dotnet ef migrations add InitialCreate

# 3. Apply Schema to SQL Server
dotnet ef database update