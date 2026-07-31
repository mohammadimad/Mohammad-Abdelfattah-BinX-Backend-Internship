# 📂 Day 04 & Day 05: ASP.NET Core API Scaffolding, Routing, Middleware & Dependency Injection

## 📝 Project Objective
The goal of this integrated lab was to build a fully functional, highly scalable, and loosely coupled **ASP.NET Core Web API** from scratch. 
Over these two days, the project was scaffolded using the modern **Minimal Hosting Model**, configured with interactive **Swagger UI** testing tools, compared across two routing models (Controllers vs. Minimal APIs), protected by a custom logging **Middleware**, and architected using **Dependency Injection (DI)**.

---

## 🛠️ Completed Architectural Tasks

### 🔹 Part 1: API Scaffolding & Visual Testing (Day 04)
- **Project Bootstrapping:** Created a clean .NET 9 Web API project and configured the unified `Program.cs` hosting pipeline.
- **Swagger UI Integration:** Integrated the `Swashbuckle.AspNetCore` engine on top of .NET 9. Resolved active reflection loading conflicts by disabling the built-in Microsoft OpenAPI generator.
- **Developer Experience (DX):** Optimized `launchSettings.json` to automatically launch the default browser directly to the interactive Swagger UI page (`/swagger/index.html`) on startup.

### 🔹 Part 2: Endpoint Design & Routing Models (Day 04)
Implemented four HTTP GET endpoints utilizing two distinct architectural routing models:
1. **Controller-Based Model (`BooksController`):**
   - Extends the framework's `ControllerBase` and decorated with `[ApiController]` and `[Route("api/[controller]")]`.
   - **Endpoint 1 (Get All):** A `[HttpGet]` action returning a mock collection of books.
   - **Endpoint 2 (Get by ID):** A parameterized `[HttpGet("{id}")]` action that binds the route variable and queries the database using LINQ.
2. **Minimal API Model (Direct Route Mapping):**
   - Mapped lightweight lambda endpoints directly in `Program.cs` to compare performance and ceremony:
     - `/api/minimal/books` (Get All)
     - `/api/minimal/books/{id}` (Get by ID)

### 🔹 Part 3: Pipeline Interception & Middleware Ordering (Day 05)
- **Custom Logging Middleware:** Implemented an inline asynchronous middleware using `app.Use()` to capture and log the HTTP `Method` and `Path` of every incoming request to the server console.
- **Pipeline Ordering Experiment:**
  - Demonstrated that registering the logging middleware *after* `app.MapControllers()` short-circuits the pipeline, preventing the logs from executing.
  - Corrected the layout by placing the logging middleware at the absolute top of the pipeline immediately after `builder.Build()`.

### 🔹 Part 4: Dependency Injection & Constructor Injection (Day 05)
- **Service Decoupling:** Created a standard contract interface `IBookService` and its concrete implementation `BookService` to abstract data retrieval logic away from the API controller.
- **DI Container Registration:** Registered the service in the built-in DI container using the **`AddScoped`** lifetime to ensure a single, safe instance is shared per HTTP request:
  ```csharp
  builder.Services.AddScoped<IBookService, BookService>();