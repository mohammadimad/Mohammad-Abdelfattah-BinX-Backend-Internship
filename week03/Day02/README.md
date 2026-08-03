# 📂 Day 02 SQL Server Schema Design & Database Normalization (3NF)

## 📝 Objective
The primary focus of Day 2 was to transition the logical API resource maps into a highly efficient, relational, and fully normalized database schema targeting Third Normal Form (3NF) in SQL Server. This design eliminates data redundancy, prevents updatedelete anomalies, enforces referential integrity, and optimizes system storage.

---

## 🛠️ Completed Schema Design

### 🔹 1. Entities & Attributes Mapping
The Library Lending System is designed around five normalized relational entities
- `Authors` Represents book creators independently to enforce 3NF.
- `Books` Represents the physical catalog assets.
- `Members` Represents registered library subscribers.
- `MemberPhones` Resolves 1NF by storing multiple phone numbers per member atomically.
- `LendingRecords` Serves as the junctionassociative table resolving the Many-to-Many relationship between Members and Books.

### 🔹 2. Normalization Process (1NF, 2NF, 3NF)
- 1NF (Atomic Values) Enforced atomic values in every column. Decoupled member phone numbers into a separate `MemberPhones` table to prevent comma-separated text values inside cells.
- 2NF (No Partial Dependency) Ensured all non-key attributes depend entirely on single surrogate primary keys (`Id`).
- 3NF (No Transitive Dependency) Eliminated transitive dependencies. Author details were moved to an independent `Authors` table (referenced via `AuthorId` in `Books`) to prevent data duplication.

---

## 🔹 3. Database Schema & Columns Configuration

#### Table `Authors`
 Column Name  Data Type  Key  Constraint  Description 
 ---  ---  ---  --- 
 Id  `INT`  `PK, IDENTITY(1,1)`  Unique author identifier 
 Name  `NVARCHAR(100)`  `NOT NULL`  Full name of the author 
 Email  `VARCHAR(100)`  `NULLABLE`  Author's contact email 

#### Table `Books`
 Column Name  Data Type  Key  Constraint  Description 
 ---  ---  ---  --- 
 Id  `INT`  `PK, IDENTITY(1,1)`  Unique book identifier 
 Title  `NVARCHAR(150)`  `NOT NULL`  The title of the book 
 Price  `DECIMAL(10,2)`  `NOT NULL`  Strict monetary precision to prevent rounding errors 
 AuthorId  `INT`  `FK - Authors(Id)`  References the author of the book 

#### Table `Members`
 Column Name  Data Type  Key  Constraint  Description 
 ---  ---  ---  --- 
 Id  `INT`  `PK, IDENTITY(1,1)`  Unique member identifier 
 FullName  `NVARCHAR(100)`  `NOT NULL`  Full legal name of the subscriber 
 Email  `VARCHAR(100)`  `NOT NULL, UNIQUE`  Unique contact email address 

#### Table `MemberPhones`
 Column Name  Data Type  Key  Constraint  Description 
 ---  ---  ---  --- 
 Id  `INT`  `PK, IDENTITY(1,1)`  Unique phone record identifier 
 MemberId  `INT`  `FK - Members(Id)`  References the phone owner 
 PhoneNumber `VARCHAR(15)`  `NOT NULL`  Member's phone number 

#### Table `LendingRecords` (Associative Junction Table)
 Column Name  Data Type  Key  Constraint  Description 
 ---  ---  ---  --- 
 Id  `INT`  `PK, IDENTITY(1,1)`  Unique transaction identifier 
 BookId  `INT`  `FK - Books(Id)`  References the borrowed book 
 MemberId  `INT`  `FK - Members(Id)` References the borrowing member 
 LendDate  `DATETIME2`  `NOT NULL`  Timestamp of checkout transaction 
 ReturnDate  `DATETIME2`  `NULLABLE`  Timestamp of return (null until returned) 

---

## 🔗 Referential Integrity (FK Constraints)
- Foreign Keys are explicitly defined across relationships
  - `Books(AuthorId)` $rightarrow$ `Authors(Id)`
  - `MemberPhones(MemberId)` $rightarrow$ `Members(Id)` (ON DELETE CASCADE)
  - `LendingRecords(BookId)` $rightarrow$ `Books(Id)`
  - `LendingRecords(MemberId)` $rightarrow$ `Members(Id)`
- Enforces Referential Integrity to prevent orphan records and ensure relational validity.

---

## 🎨 Entity Relationship Diagram (ERD)
The database ERD schema was diagrammed using dbdiagram.io representing table schemas, keys, constraints, and relationships. 

![ERD Diagram](.erd.png)