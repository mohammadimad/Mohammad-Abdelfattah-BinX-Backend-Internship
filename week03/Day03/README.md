# 📂 Day 03: Entity Framework Core Setup & Code-First Migrations

## 📝 Objective
The primary focus of Day 3 was to integrate **Entity Framework Core (EF Core)** as our Object-Relational Mapper (ORM), bridging our plain C# domain models with a physical SQL Server database. This lab covers installing database provider packages, designing database context classes (`DbContext`) and entities with navigation properties, securing connection strings, and executing code-first migrations to deploy the normalized Day 2 schema to SQL Server.

---

## 🧠 Core Architectural Concepts Learned

### 1. What is an ORM & Why EF Core?
An Object-Relational Mapper (ORM) abstracts raw SQL commands, allowing developers to interact with relational databases using object-oriented code. **EF Core** acts as a compiler and translator:
- It maps C# classes (Entities) to database tables.
- It translates strongly-typed **LINQ queries** into highly optimized, native SQL statements.
- It manages connection pooling and tracks changes automatically.

### 2. The Role of DbContext & DbSet
The `DbContext` is the heart of EF Core, representing a session with the database.
- **`DbSet<TEntity>`**: Represents a collection of a specific entity in the database that can be queried and saved.
- **Change Tracker:** EF Core monitors the state of every entity loaded through the `DbContext` (Unchanged, Added, Modified, Deleted) and translates these states into targeted SQL DML statements upon calling `SaveChangesAsync()`.

### 3. Code-First Migrations Workflow
Code-First means C# entity classes are the **Single Source of Truth**. The database schema is derived directly from the codebase:
1. `dotnet ef migrations add <Name>`: Inspects the `DbContext` and entity classes, comparing them against the previous migration state, and generates a C# migration file describing the DDL changes.
2. `dotnet ef database update`: Reads the generated migration scripts, compiles them into native SQL statements, and executes them inside SQL Server to apply the physical schema.

### 4. Connection String Security (A Vital Lesson) ⚠️
- Storing real credentials, user IDs, or passwords inside code files or committing them to a public GitHub repository is a severe security incident.
- **Best Practice:** Local development values are stored in `appsettings.Development.json` (which is excluded from Git via `.gitignore`). Production connection strings must always be supplied through environment variables or secure vault managers, never committed to source control.

---

## 💻 Code Implementation: Database Context & Entities

Here is the architectural C# blueprint of our normalized **Library Lending System** schema mapped to EF Core:

### 1. Entity Classes (Models)

```csharp
using System;
using System.Collections.Generic;

public class Book
{
    public int Id { get; set; } // Automatically recognized as Primary Key (PK)
    public string Title { get; set; } = null!;
    public decimal Price { get; set; }
    
    // Navigation Property representing the Many-to-Many join table
    public List<LendingRecord> LendingRecords { get; set; } = new();
}

public class Member
{
    public int Id { get; set; } // Primary Key (PK)
    public string Name { get; set; } = null!;
    public DateTime JoinedDate { get; set; }

    // Navigation Property
    public List<LendingRecord> LendingRecords { get; set; } = new();
}

public class LendingRecord
{
    public int Id { get; set; } // Primary Key (PK)
    public int BookId { get; set; } // Foreign Key (FK)
    public int MemberId { get; set; } // Foreign Key (FK)
    public DateTime LendingDate { get; set; }
    public DateTime? ReturnDate { get; set; } // Nullable (null until returned)

    // Navigation Properties representing relational integrity
    public Book Book { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
## 2. The Database Context (LibraryDbContext)

```csharp
using Microsoft.EntityFrameworkCore;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<LendingRecord> LendingRecords => Set<LendingRecord>();
}
```

## 3. Service Registration (Program.cs)

```csharp
// Registering the DbContext in the DI Container with SQL Server Provider
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

## 🛠️ Hands-On Lab: Set Up EF Core & Run First Migration

### NuGet Packages Installed:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Generated Migration Blueprint:
Inspected the context and compiled the initial schema snapshot:

```bash
dotnet ef migrations add InitialCreate
```

### Database Schema Deployment:
Executed migration and deployed tables directly to local SQL Server (LocalDB):

```bash
dotnet ef database update
```

### Verification:
Successfully verified the creation of Books, Members, and LendingRecords tables, including PKs, FKs, and constraints using SQL Server Management Studio (SSMS).
