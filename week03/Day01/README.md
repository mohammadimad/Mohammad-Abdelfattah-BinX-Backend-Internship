# 📂 Day 01: REST API Design Principles & Resource Modeling

## 📝 Objective
The primary focus of Day 1 was to master the core architectural constraints of **REST (Representational State Transfer)**, moving away from procedural RPC-style APIs ("JSON over HTTP") to structured, resource-oriented endpoint design. This lab establishes standardized resource naming conventions, semantic HTTP status code routing, and long-term API versioning strategies.

---

## 🧠 Core Architectural Concepts Learned

### 1. REST vs. RPC (Action vs. Resource)
- **RPC Style (Anti-Pattern):** Designing endpoints around actions (e.g., `POST /api/createBook`, `GET /api/getBooks`). This violates standard web protocols by putting verbs inside URLs.
- **RESTful Style (Best Practice):** Designing endpoints around **Nouns (Resources)** (e.g., `/api/books`). The action is entirely dictated by the **HTTP Verb** (GET, POST, PUT, DELETE), ensuring a clean, uniform interface.
- **Statelessness:** No session state is held on the server between requests. Each incoming request must carry all the metadata and credentials needed to process it, enabling seamless horizontal scalability across multiple server nodes.

### 2. The "Fake 200 OK" Anti-Pattern
- Returning an HTTP status code of `200 OK` with a custom error body (e.g., `{"success": false, "error": "Not Found"}`) is a severe violation of REST design. 
- It breaks standard HTTP middleware, CDN caching, reverse proxies, and API gateways. 
- In REST, the **HTTP status code itself must communicate the outcome** (e.g., returning a true `404 Not Found` or `400 Bad Request` at the protocol level).

### 3. API Versioning Strategy
- Integrating versioning early (e.g., via URL segment `/api/v1/`) protects existing consumers (like mobile apps) from breaking changes when database schemas or response payloads evolve.

---

## 🛠️ Hands-On Lab: REST Resource Map (Library Catalog)

- **Selected Domain:** Library Catalog System
- **Core Plural Resources:** `books` (الكتب), `members` (الأعضاء), `reviews` (مراجعات الكتب).
- **Versioning Convention:** URL-Based Segment (`/v1/`).

### 📊 API Endpoints Routing Map

| HTTP Method | Endpoint Path | Success Code | Error Code (Min 1) | Description / Intent |
| :--- | :--- | :---: | :---: | :--- |
| **GET** | `/api/v1/books` | `200 OK` | `400 Bad Request` | Retrieves a list of all books in the catalog. |
| **GET** | `/api/v1/books/{id}` | `200 OK` | `404 Not Found` | Retrieves a single book's details by its ID. |
| **POST** | `/api/v1/books` | `201 Created` | `400 Bad Request` | Scaffolds and creates a new book record. *Requires Location Header.* |
| **PUT** | `/api/v1/books/{id}` | `200 OK` | `404 Not Found` | Replaces/updates an existing book's details entirely. |
| **DELETE** | `/api/v1/books/{id}` | `204 No Content`| `404 Not Found` | Deletes a book from the catalog permanently. |
| **GET** | `/api/v1/books/{id}/reviews`| `200 OK` | `404 Not Found` | **(Nested):** Retrieves all user reviews belonging to a specific book. |

### 🔍 Status Code Architectural Justifications:
*   **`201 Created`**: Returns when a `POST` is successful, accompanied by a `Location` header pointing to the URI of the newly created resource (e.g., `Location: /api/v1/books/105`).
*   **`204 No Content`**: Returns when a `DELETE` is successful. Returning empty bodies saves network bandwidth since there is no resource left to represent.
*   **`400 Bad Request`**: Returns when client-side payload validation fails (e.g., missing required fields like `Title` during creation).
*   **`404 Not Found`**: Returns when querying or acting upon an ID that does not exist in the database, avoiding unhandled null reference crashes.

---

## 📂 Deliverables & Workspace Verification
- [x] REST Resource design map with comprehensive endpoints.
- [x] Validated semantic HTTP status codes for success and failure paths.
- [x] Documented API versioning convention (`v1`).
- [x] Day 01 Design Map assembled in Notion ready for the mentor check-in.