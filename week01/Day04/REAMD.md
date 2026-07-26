# 📂 Day 04: C# Fundamentals III - Collections, LINQ & Async/Await

## 📝 Objective

The primary focus of Day 4 was to optimize backend performance, write declarative clean queries, and ensure runtime scalability. This was achieved by evaluating collection time complexities, writing fluent LINQ queries, implementing non-blocking asynchronous APIs, and establishing robust exception handling.

---

## 🛠️ Completed Tasks

1. **Collection Optimization:**
   - Evaluated data access patterns and memory structures: `List<T>` (ordered sequential), `Dictionary<TKey, TValue>` ($O(1)$ fast key-lookup), and `HashSet<T>` ($O(1)$ fast uniqueness validation).

2. **Fluent LINQ Queries:**
   - Designed 3 declarative queries against a populated list of domain models using Method Syntax:
     - **Filter:** Used `.Where()` to extract criteria-specific subsets.
     - **Projection:** Used `.Select()` to transform objects into lightweight data projections.
     - **Aggregation:** Used mathematical methods like `.Average()` or `.Count()`.
   - Analyzed the performance benefits of **Deferred Execution**.

3. **Asynchronous Non-Blocking APIs:**
   - Engineered an asynchronous method returning `Task<T>` that simulates I/O-bound delay via:
     ```csharp
     await Task.Delay(2000);
     ```
   - Freed execution threads back to the Thread Pool to enhance server scalability.
   - Implemented safe consumption using `await` from an asynchronous entry point.

4. **Robust Exception Handling:**
   - Created a defensive input-processing workflow wrapped in a `try-catch` block.
   - Targeted specific exceptions (`FormatException`) to recover from user parse failures without crashing the application process.
