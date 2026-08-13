# 📂 Day 05: Middleware Pipeline & Dependency Injection

## 📝 Objective
The primary focus of Day 5 was to master the request lifecycle in ASP.NET Core. This lab explores the **Middleware Pipeline** execution order and implements **Dependency Injection (DI)** to achieve Inversion of Control (IoC), ensuring that controllers remain loosely coupled, testable, and completely independent of concrete service implementations.

---

## 🧠 Core Architectural Concepts

### 1. The Middleware Pipeline (Request Flow)
- Every incoming HTTP request passes through a chain of middleware components configured in `Program.cs`.
- **Order is Critical:** Middleware executes in the exact order it is registered. For instance, placing `UseAuthorization()` before `UseAuthentication()` causes logical failures since the system cannot authorize an unidentified user.
- **Custom Middleware:** We can intercept requests to perform cross-cutting concerns (like logging or error handling) before passing the request to the next component via `await next()`.

### 2. Dependency Injection & Service Lifetimes
ASP.NET Core provides a built-in DI container to manage the instantiation and disposal of services. Choosing the correct lifetime prevents severe memory leaks and state corruption:
- **Transient (`AddTransient`):** A new instance is created every single time it is requested. Best for lightweight, stateless services.
- **Scoped (`AddScoped`):** A single instance is created per HTTP request and shared across all components during that request. *This is the enterprise standard for data access and repositories.*
- **Singleton (`AddSingleton`):** A single instance is created once and shared globally for the entire lifetime of the application. Dangerous if used with stateful services like Entity Framework's `DbContext`.

### 3. Constructor Injection
- By requesting interfaces (e.g., `IOrderService`) through a controller's constructor, the DI container automatically resolves and supplies the concrete implementation at runtime.
- **Why?** It completely eliminates the `new` keyword inside controllers, making the system modular and unit-testable (allowing mock services to be swapped in easily).

---

## 🛠️ Hands-On Lab Deliverables

The following architectural tasks were successfully implemented and tested:

- [x] **Custom Logging Middleware:** Engineered an inline asynchronous middleware (`app.Use`) that successfully intercepts incoming requests and logs the HTTP Method and Path to the console.
- [x] **Pipeline Ordering Experiment:** Deliberately misplaced the custom middleware after endpoint mapping to observe the short-circuiting effect, then corrected the order to restore proper execution.
- [x] **Service Decoupling:** Designed a business logic contract (`Interface`) and its concrete implementation class.
- [x] **DI Registration:** Successfully registered the service in `Program.cs` using the `AddScoped` lifetime.
- [x] **Constructor Injection:** Refactored the API Controller to inject the service via its constructor, successfully serving API endpoints without tightly coupling to the concrete class.

---
*Prepared by **[Mohammad Abdelfattah]** as part of the BinX Tech Backend Internship Program.*