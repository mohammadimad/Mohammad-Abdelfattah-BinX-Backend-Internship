# 📂 Day 04: Implementing Asynchronous CRUD Operations with EF Core

## 📝 Objective
The primary focus of Day 4 was to implement full asynchronous **CRUD (Create, Read, Update, Delete)** operations on our primary resource (`Books`) backed by SQL Server via Entity Framework Core. This lab demonstrates how to leverage non-blocking async queries, optimize database reads using change tracking overrides, handle model validation errors, and output semantic HTTP status codes.

---

## 🧠 Core Architectural Concepts Learned

### 1. Non-Blocking Asynchronous DB Access
In database-driven backend services, waiting for database disk I/O blocks standard execution threads if executed synchronously. 
- Using asynchronous operations (`ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`) releases the active execution thread back to the **Thread Pool** while waiting for SQL Server to respond.
- This secures high throughput, allowing a single server instance to handle thousands of concurrent requests.

### 2. EF Core Change Tracker & `AsNoTracking()`
EF Core automatically tracks the state of every entity loaded through the `DbContext` to detect modifications and generate optimal SQL statements:
- **`SaveChangesAsync()`:** Generates targeted `UPDATE` statements changing only the modified columns, rather than rewriting the entire row.
- **`AsNoTracking()` (Read Optimization):** For read-only endpoints (like Get-All), calling `.AsNoTracking()` skips the overhead of change tracking entirely. This saves significant RAM and CPU cycles on heavy data reads.

### 3. Defensive Programming: Null Checks & Validation
- **Null Safety (404):** Querying non-existent IDs must return a clean `NotFound()` (404) at the protocol level rather than allowing a null reference exception to propagate and crash the server thread.
- **Model Validation (400):** Catching invalid input payloads (e.g., negative prices, empty titles) using `ModelState.IsValid` and returning `BadRequest(ModelState)` (400) before any redundant database connections are opened.

### 4. The Unawaited Task Pitfall ⚠️
Forgetting the `await` keyword before async calls (like `_context.SaveChangesAsync()`) compiles without errors but causes silent concurrency bugs. The compiler warnings regarding unawaited tasks must always be treated as blocker bug reports.

---

## 💻 Code Implementation: Full Asynchronous Books Controller

Here is the clean, Web API-compliant C# implementation of our **`BooksController`** inheriting from `ControllerBase`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Day03.Domain;
using Day03.Data;

[ApiController]
[Route("api/v1/[controller]")]
public class BooksController : ControllerBase
{
    private readonly LibraryDbContext _context;

    public BooksController(LibraryDbContext context)
    {
        _context = context;
    }

    // 1. READ: Get All Books (AsNoTracking Optimized)
    [HttpGet]
    public async Task<IActionResult> GetAll()    
    {
        var books = await _context.Books.AsNoTracking().ToListAsync();
        return Ok(books);
    }

    // 2. READ: Get Book By ID (Null-Safe 404 check)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        
        if (book == null)
        {
            return NotFound(); // Returns 404
        }

        return Ok(book);
    }

    // 3. CREATE: Add New Book (201 Created with Location Header)
    [HttpPost]
    public async Task<IActionResult> Create(Book book)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // Returns 400
        }

        _context.Books.Add(book);
        await _context.SaveChangesAsync(); // Persists and generates Database ID

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    // 4. UPDATE: Edit Existing Book (Targeted Change Tracking)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Book bookInput)
    {
        if (id != bookInput.Id || !ModelState.IsValid)
        {
            return BadRequest();
        }

        var existingBook = await _context.Books.FindAsync(id);
        if (existingBook == null)
        {
            return NotFound();
        }

        // Apply changes (EF Core detects modifications automatically)
        existingBook.Title = bookInput.Title;
        existingBook.Price = bookInput.Price;

        await _context.SaveChangesAsync();
        return NoContent(); // Returns 204
    }

    // 5. DELETE: Remove Book (Null-Safe 404 Check)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        
        return NoContent(); // Returns 204
    }
}
## 🧪 Postman API Testing Matrix

All 5 endpoints were tested and validated successfully on local host, including deliberate failure paths:

| HTTP Method | Endpoint Path | Payload / Parameter | Expected Status Code | Actual Outcome |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/v1/books` | Valid JSON | `201 Created` | Successful creation, returns Location header |
| **POST** | `/api/v1/books` | Invalid JSON (null title) | `400 Bad Request` | Handled by ModelState validation |
| **GET** | `/api/v1/books` | None | `200 OK` | Retrieves array of all books |
| **GET** | `/api/v1/books/{id}` | Existing ID (e.g., 1) | `200 OK` | Retrieves specific book payload |
| **GET** | `/api/v1/books/{id}` | Non-existing ID (e.g., 999) | `404 Not Found` | Handled gracefully without crash |
| **PUT** | `/api/v1/books/{id}` | Existing ID + payload | `204 No Content` | Changes persisted successfully |
| **DELETE** | `/api/v1/books/{id}` | Existing ID | `204 No Content` | Record deleted from SQL database |
