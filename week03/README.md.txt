# 📂 Week 3: REST APIs, Entity Framework Core & SQL Server

## 📝 Week Overview
Week 3 represents the core transition from in-memory hardcoded datasets to persistent relational database systems. This week was dedicated to mastering **RESTful API design principles**, modeling database schemas using **Third Normal Form (3NF) normalization**, configuring **Entity Framework Core (EF Core)** as our ORM, implementing asynchronous CRUD operations, and performing end-to-end integration testing using **Postman Collections and Environments**.

---

## 📂 Week 3 Folder Directory (Flat Architecture)

The workspace is organized hierarchically as a modular flat architecture, with each directory housing a self-contained deliverable or project:

```text
Week03/
├── README.md                  # This Week 3 Main Documentation
│
├── Day01/                     # Day 01: REST API Design & Resource Modeling
│   └── README.md              # REST Resource Map (Library Catalog) & Versioning Doc
│
├── Day02/                     # Day 02: SQL Server Schema Design & Normalization
│   └── README.md              # 3NF Table Schemas, Data Types, and ERD Reference
│
├── Day03/                     # Day 03: EF Core Setup & Code-First Migrations
│   ├── LibraryDbContext.cs    # EF Core Central DbContext
│   ├── Models/                # Entity Classes (Book, Member, LendingRecord)
│   ├── Migrations/            # EF Core Generated Schema Migrations
│   └── README.md              # Setup Notes & CLI Commands Documentation
│
├── Day04/                     # Day 04: Async CRUD Operations with EF Core
│   ├── Controllers/           # BooksController & MembersController (Web API)
│   └── README.md              # Async CRUD Logic, Change Tracking & Null-Safe Checks
│
└── Day05/                     # Day 05: API Testing & Documentation with Postman
    ├── LibrarySystem.postman_collection.json  # Exported Postman Test Suite
    └── README.md              # Verified Test Matrix, Environment Variables & Scripts

## 🗓️ Weekly Progress & Learning Achievements

### 🔹 Module 1: REST API Design & Resource Modeling (Day 01)
* **Focus:** Transitioned from RPC-style action endpoints (e.g., `/createBook`) to resource-oriented nouns (`/api/v1/books`).
* **Deliverables:** Scaffolded a comprehensive REST Resource Map with semantic HTTP status codes (`201 Created` with Location headers, `204 No Content` for deletes, and `404`/`400` error paths) under URL versioning (`v1`).

### 🔹 Module 2: SQL Server Schema Design & Normalization (Day 02)
* **Focus:** Applied 1NF (atomic values), 2NF, and 3NF to eliminate data redundancy, prevent update/delete anomalies, and enforce referential integrity.
* **Deliverables:** Designed an optimized 3NF Library Schema using precise column types (`DECIMAL(18,2)` for monetary attributes to avoid floating-point rounding errors) and mapped the Many-to-Many Lending associative table.

### 🔹 Module 3: EF Core Setup & Code-First Migrations (Day 03)
* **Focus:** Installed SQL Server and Tools NuGet providers, mapped our 3NF schema to C# Entities and `DbContext`, and executed Code-First migrations.
* **Deliverables:** Applied the `InitialCreate` migration to physically deploy database tables in SQL Server (LocalDB) and implemented secure gitignored connection string practices.

### 🔹 Module 4: Asynchronous CRUD Operations (Day 04)
* **Focus:** Engineered non-blocking async queries (`ToListAsync()`, `SaveChangesAsync()`) to optimize Thread Pool utilization, and utilized `.AsNoTracking()` to bypass Change Tracking overhead on read-only queries.
* **Deliverables:** Developed robust, Web API-compliant controllers with full CRUD endpoints returning semantic JSON payloads.

### 🔹 Module 5: Postman Testing & Integration (Day 05)
* **Focus:** Automated API integration testing, created reusable test suites, and leveraged environmental portability.
* **Deliverables:** Created a complete Postman collection featuring environmental variables (`{{baseUrl}}`) and automated JavaScript test scripts to assert expected status codes on both happy and sad paths.

---

## ⚙️ Technical Stack

* **Web Framework:** ASP.NET Core Web API (.NET 9.0)
* **ORM & Database:** Entity Framework Core, SQL Server (LocalDB)
* **Database Tooling:** SSMS / Azure Data Studio, dbdiagram.io
* **API Testing:** Postman (Collections, Environments, Test Scripts)
* **Version Control:** Git & GitHub

***

**Prepared by:** [Mohammad Abdelfattah]  
**Position:** Intern Developer at BinX Tech
