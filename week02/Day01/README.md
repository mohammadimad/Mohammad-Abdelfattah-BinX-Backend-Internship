# 📂 Day 01: Generics & Advanced Collections

## 📝 Objective
The primary focus of Day 1 was to master compile-time type safety and code reusability in C# using **Generics**. This lab demonstrates how to eliminate expensive boxing/unboxing operations in memory, apply structural type constraints (`where T : class`), design a reusable data store, and protect internal collection states from mutability leaks using the least permissive interfaces.

---

## 🛠️ Completed Lab Tasks

1. **Generic Repository Pattern (`Repository<T>`):**
   - Designed a generic, in-memory data store class `Repository<T>` equipped with fundamental CRUD capabilities: `Add`, `GetAll`, and `Find`.
   - Integrated functional programming by designing the `Find` method to accept a predicate delegate:
     ```csharp
     Func<T, bool> predicate
     ```
     This allows callers to write custom lambda filters evaluated at runtime via `.FirstOrDefault()`.

2. **Structural Type Constraints (`where`):**
   - Applied a strict structural constraint to the class definition:
     ```csharp
     where T : class
     ```
   - This restricts the repository to reference types (entities) only, ensuring safety against value types and unlocking the ability to return `null` safely upon lookup failures.

3. **Multi-Type Reusability Testing:**
   - Proved the absolute reusability of the generic template by instantiating the repository against two distinct domain entities from Week 1: `Book<int>` and `Member<string>` inside the application entry point.

4. **Mutability Leak Protection:**
   - Refactored `GetAll()` to return **`IReadOnlyList<T>`** using the built-in `.AsReadOnly()` wrapper.
   - This design enforces encapsulated read-only protection, guaranteeing that calling layers cannot bypass repository business logic to modify, clear, or corrupt the internal list.
