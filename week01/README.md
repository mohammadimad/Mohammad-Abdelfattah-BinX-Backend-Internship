# 🚀 BinX Tech Backend Development Internship

## 📝 Week 1: Onboarding & Foundations

Welcome to the documentation for **Week 1** of Phase 1 of the BinX Tech Backend Development Internship.

This week was focused on establishing a rock-solid development environment, mastering advanced C# language core fundamentals (including memory management, object-oriented design, asynchronous patterns, and collections), and learning the industry-standard Git feature-branch workflow.

---

## 📁 Week 1 Directory Structure

The deliverables for this week are modularly organized into daily folders:

```text
Week01/
├── README.md               # This Week 1 Summary Document
├── Day01/
│   └── HelloBinX/          # .NET SDK & IDE Setup, First Console App
├── Day02/
│   └── Day02Practice/      # Value vs. Reference Types, Switch Expressions, Null Safety
├── Day03/
│   └── LibrarySystem/      # OOP Domain Design, Encapsulation, Interfaces, Records
└── Day04/
    └── CollectionsAndAsync/ # List/Dict/HashSet, LINQ Queries, Async/Await, try-catch
```

---

## 📊 Daily Progress & Learning Objectives

### 🗓️ Day 01: Onboarding & Environment Setup

- **Focus Topic:** `SETUP & TOOLING`
- **Core Concepts:** .NET 8.0 SDK (LTS) environment verification, Visual Studio 2022 setup, and global `.gitignore` configuration.
- **Deliverable:** Flat console solution printing active developer metadata and current timestamp.

### 🗓️ Day 02: C# Fundamentals I (Memory & Type Safety)

- **Focus Topic:** `CORE LANGUAGE`
- **Core Concepts:** Stack (Value Types) vs. Heap (Reference Types) allocation, Relational Pattern Matching using C# Switch Expressions, and safe Nullable Reference Types (NRT) input handling.
- **Deliverable:** Standard project demonstrating copy-by-value vs. copy-by-reference mutation behavior, a grade classifier, and safe null checks.

### 🗓️ Day 03: C# Fundamentals II (Object-Oriented Programming)

- **Focus Topic:** `OOP & ARCHITECTURE`
- **Core Concepts:** Domain modeling using identity-based Classes vs. immutable value-based Records (DTOs), applying strict state encapsulation, and implementing polymorphic contracts using Interfaces instead of tight inheritance.
- **Deliverable:** Library domain model where unrelated assets (`Book` and `Laptop`) implement the shared `ILendable` interface.

### 🗓️ Day 04: C# Fundamentals III (Collections, LINQ & Async)

- **Focus Topic:** `ASYNC & COLLECTIONS`
- **Core Concepts:** Time complexities of List, Dictionary, and HashSet ($O(1)$ / $O(n)$); fluent LINQ queries (Method Syntax); non-blocking asynchronous APIs (`Task.Delay`); and targeted exception handling (`FormatException`).
- **Deliverable:** Integrated database-simulation program utilizing LINQ filters, projections, aggregations, and safe async workflows.

---

## ⚙️ Applied Technical Stack

- **Language & Runtime:** C# 12 | .NET 8.0 SDK (LTS)
- **Tools & IDE:** Visual Studio 2022 | Git | GitHub

---

👤 **Prepared by:** Mohammad Abdelfattah
