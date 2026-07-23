# 📂 Day 02: C# Fundamentals I - Types, Variables & Control Flow

## 📝 Objective

The primary focus of Day 2 was to master memory management foundations in C# by distinguishing between value types and reference types, utilizing modern pattern-matching expressions, and writing secure, null-safe backend code.

---

## 🛠️ Completed Tasks

1. **Memory Type Analysis:**
   - Declared and analyzed 3 **Value Types** (stored on the Stack) and 3 **Reference Types** (stored on the Heap).
   - Evaluated active metadata types at runtime using:
     ```csharp
     variable.GetType()
     ```

2. **Copy Behavior & Mutation Lab:**
   - Implemented a console experiment demonstrating that copying Value Types duplicates the actual data in memory, keeping variables isolated.
   - Demonstrated that copying Reference Types duplicates only the pointer on the Stack, leading to shared object mutation in the Heap.

3. **Pattern Matching with Switch Expressions:**
   - Engineered a clean, expression-bodied grade classifier method.
   - Leveraged modern C# **Relational Patterns** (`>=`) and enforced exhaustive evaluation with the **Discard Pattern** (`_`).

4. **Safe Nullable Input Processing:**
   - Implemented a classical `if-else` verification program to read and process keyboard inputs safely.
   - Handled compiler warnings under **Nullable Reference Types (NRT)** enabled by default in .NET 9 without using the unsafe null-forgiving operator (`!`).
