# 📂 Week 1: Onboarding, C# Fundamentals & Git Workflow

## 📝 Week Overview

Week 1 marks the beginning of **Phase 1: Onboarding & Foundations** of the BinX Tech Backend Development Internship. This week was dedicated to establishing a stable, verified .NET development environment and mastering C# language core fundamentals.

Adhering to clean repository standards, every day is structured as a **Flat Project Solution** directly inside its respective day folder to ensure clean, direct readability during code reviews.

---

## 📂 Week 1 Folder Directory (Flat Architecture)

The workspace is organized as follows, where each day represents a self-contained Visual Studio solution directly under its directory:

```text
Week01/
├── README.md                  # This Week 1 Comprehensive Summary
│
├── Day01/                     # Day 01: Environment Setup & First Console App
│   ├── Day01.sln              # Visual Studio Solution
│   ├── Day01.csproj           # C# Project File
│   ├── Program.cs             # Application Entry Point
│   └── README.md              # Day 01 Documentation
│
├── Day02/                     # Day 02: Memory, Types & Control Flow
│   ├── Day02.sln
│   ├── Day02.csproj
│   ├── Program.cs
│   └── README.md              # Day 02 Documentation
│
├── Day03/                     # Day 03: OOP & Domain Modeling
│   ├── Day03.sln
│   ├── Day03.csproj
│   ├── Program.cs
│   └── README.md              # Day 03 Documentation
│
└── Day04/                     # Day 04: Collections, LINQ & Async Task Delay
    ├── Day04.sln
    ├── Day04.csproj
    ├── Program.cs
    └── README.md              # Day 04 Documentation
🗓️ Daily Progress & Learning Objectives
🔹 Day 01: Onboarding & Environment Setup

Focus: .NET 8.0 SDK (LTS) environment verification, Visual Studio 2022 setup, and global .gitignore configuration.

Deliverable: Flat console solution printing active developer metadata and current timestamp.

🔹 Day 02: C# Fundamentals I (Memory & Type Safety)

Focus: Stack (Value Types) vs. Heap (Reference Types) allocation, Relational Pattern Matching using C# Switch Expressions, and safe Nullable Reference Types (NRT) input handling.

Deliverable: Standard project demonstrating copy-by-value vs. copy-by-reference mutation behavior, a grade classifier, and safe null checks.

🔹 Day 03: C# Fundamentals II (Object-Oriented Programming)

Focus: Domain modeling using identity-based Classes vs. immutable value-based Records (DTOs), applying strict state encapsulation, and implementing polymorphic contracts using Interfaces instead of tight inheritance.

Deliverable: Library domain model where unrelated assets (Book and Laptop) implement the shared ILendable interface.

🔹 Day 04: C# Fundamentals III (Collections, LINQ & Async)

Focus: Time complexities of List, Dictionary, and HashSet (O(1) vs. O(n)); fluent LINQ queries (Method Syntax); non-blocking asynchronous APIs (Task.Delay); and targeted exception handling (FormatException).

Deliverable: Integrated database-simulation program utilizing LINQ filters, projections, aggregations, and safe async workflows.

⚙️ Applied Technical Stack
Language & Runtime: C# 12, .NET 8.0 SDK (LTS)

IDE: Visual Studio 2022

Version Control: Git & GitHub

Prepared by Mohammad AbdAlfattah - Intern Developer at BinX Tech.
```
