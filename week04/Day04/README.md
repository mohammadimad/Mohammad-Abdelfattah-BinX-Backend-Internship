# 📂 Day 04: Input Validation with FluentValidation

## 📝 Objective

The primary focus of Day 4 was to establish a robust, decoupled **Data Validation Layer** using **FluentValidation**. This lab demonstrates how to move away from cluttering data models with DataAnnotations, enforcing strict business rules before the request reaches the controller, and standardizing error responses.

---

## 🧠 Architectural Decisions & Concepts

### 1. Separation of Concerns (FluentValidation vs. DataAnnotations)

- **Decision:** Adopted FluentValidation over traditional DataAnnotations.
- **Why?** DataAnnotations violate the Single Responsibility Principle (SRP) by mixing validation logic directly inside DTO models. FluentValidation abstracts this logic into dedicated validator classes (`AbstractValidator<T>`), allowing for complex, testable, and chainable business rules.

### 2. The "Fail-Fast" Pipeline Integration

- Integrated FluentValidation directly into the ASP.NET Core Dependency Injection (DI) pipeline.
- **Outcome:** Invalid payloads are intercepted during the **Model Binding** phase. The pipeline short-circuits and immediately returns a `400 Bad Request`, shielding controllers and the database from processing corrupted states.

### 3. Standardized Error Responses

- Automatically formatted all validation failures into the **RFC 7807 `ValidationProblemDetails`** standard.
- This ensures frontend clients receive a structured JSON payload detailing exactly which properties failed and the specific business rule violated.

---

## 🛠️ Hands-On Lab Deliverables

The following tasks were successfully implemented and tested in the codebase:

- [X]  **Package Integration:** Installed and configured `FluentValidation.AspNetCore` in `Program.cs`.
- [X]  **Create Request Validator:** Engineered a dedicated validator for the primary resource creation DTO, covering multiple real-world business constraints (e.g., minimum values, maximum string lengths, and logical ranges).
- [X]  **Update Request Validator:** Engineered a dedicated validator for the update DTO logic.
- [X]  **Postman Verification:** Verified that the pipeline successfully intercepts bad requests. Tested both happy paths and deliberate validation failures, confirming the return of structured `400 Bad Request` responses with specific error messages.

---

*Prepared by **[Your Name]** as part of the BinX Tech Backend Internship Program.*
