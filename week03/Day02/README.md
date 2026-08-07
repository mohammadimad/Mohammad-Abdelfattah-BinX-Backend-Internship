# 📂 Day 02: SQL Server Schema Design & Database Normalization (3NF)

## 📝 Objective
The primary focus of Day 2 was to translate our logical API resource models into a physically persistent, highly optimized relational database schema in SQL Server. This lab covers the rigorous application of the **Three Normal Forms (1NF, 2NF, 3NF)**, establishing strict referential integrity through Primary and Foreign Keys, and selecting memory-efficient column data types to protect financial and system data integrity.

---

## 🧠 Core Architectural Concepts Learned

### 1. Database Normalization (Why Normalize?)
Designing a relational database without normalization leads to data redundancy, massive storage waste, and severe data anomalies:
- **Insert Anomaly:** Inability to insert a record because some unrelated data is missing.
- **Update Anomaly:** Updating a record (like a member's name) in one row but missing other duplicated rows, leaving the database in an inconsistent state.
- **Delete Anomaly:** Deleting a record (like deleting a loan transaction) which accidentally wipes out the entire member's profile.

### 2. The Three Normal Forms (1NF, 2NF, 3NF) under the Hood
- **First Normal Form (1NF - Atomicity):** Requires that every column contains atomic (indivisible) values. Storing nested lists or comma-separated values in a single cell (e.g., `Phones: "111, 222"`) is strictly prohibited.
- **Second Normal Form (2NF - Partial Dependency):** Requires that every non-key column must depend on the *entire* primary key, eliminating partial dependencies (primarily relevant with composite keys).
- **Third Normal Form (3NF - Transitive Dependency):** Requires that non-key columns must depend *only* on the primary key, and not on other non-key columns. Storing an author's email inside the `Books` table violates 3NF, requiring the extraction of an `Authors` table.

### 3. Precision Over Performance: The Monetary Float Danger
- **The Float Trap:** Using `FLOAT` or `REAL` types for monetary/financial values is a severe bug. Floating-point types use approximate binary representations, resulting in tiny, cumulative rounding errors (e.g., `0.1 + 0.2 = 0.300000000004`), which is unacceptable for audit trials and financial calculations.
- **The Architect's Choice:** Always use **`DECIMAL(18,2)`** in SQL Server for monetary values, securing absolute, non-rounded decimal precision.
- **Storage Efficiency:** Choose narrow column types: `INT` for standard numeric IDs, and sized `NVARCHAR(100)` rather than unbounded `NVARCHAR(MAX)` to optimize indexing speed and disk page utilization.

---

## 🛠️ Hands-On Lab: Normalized Library Schema (3NF Design)

- **Selected Domain:** Library Lending System
- **Applied Normalization:** Third Normal Form (3NF)

### 📊 Normalized Relational Table Schemas

#### 1. Table: `Books` (Primary Catalog Entity)
- **`Id`** (`INT`, Primary Key, `IDENTITY(1,1)`): Unique book auto-increment identifier.
- **`Title`** (`NVARCHAR(150)`, `NOT NULL`): The title of the catalog asset.
- **`Price`** (`DECIMAL(18,2)`, `NOT NULL`): Standard price of the book.

#### 2. Table: `Members` (User Registry Entity)
- **`Id`** (`INT`, Primary Key, `IDENTITY(1,1)`): Unique member auto-increment identifier.
- **`Name`** (`NVARCHAR(100)`, `NOT NULL`): Full name of the library member.
- **`JoinedDate`** (`DATETIME2`, `NOT NULL`): Registration timestamp.

#### 3. Table: `LendingRecords` (Many-to-Many Associative Join Table)
- **`Id`** (`INT`, Primary Key, `IDENTITY(1,1)`): Unique loan transaction identifier.
- **`BookId`** (`INT`, Foreign Key `-> Books(Id)`, `NOT NULL`): References the borrowed book.
- **`MemberId`** (`INT`, Foreign Key `-> Members(Id)`, `NOT NULL`): References the borrowing member.
- **`LendingDate`** (`DATETIME2`, `NOT NULL`): Timestamp of the checkout transaction.
- **`ReturnDate`** (`DATETIME2`, `NULLABLE`): Timestamp of return (remains null until the book is returned).

---

## 🔗 Referential Integrity Constraints
- Foreign keys are explicitly defined on `LendingRecords(BookId)` and `LendingRecords(MemberId)`.
- This enforces **Referential Integrity** at the engine level, preventing orphan rows (e.g., blocking the checkout of a non-existent book, and preventing the deletion of a member who has active pending borrows).

---

## 🎨 Entity Relationship Diagram (ERD)
The database ERD schema was diagrammed using **dbdiagram.io** representing the table schemas, keys, constraints, and relationships.
*(Attach your exported ERD diagram image here `![ERD Diagram](./erd.png)`)*