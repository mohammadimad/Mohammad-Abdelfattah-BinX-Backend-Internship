# Week 7 - Day 5: Sprint 2 Close-Out & Retrospective

## Day Overview

Day 5 focused on conducting our Sprint 2 close-out review [9]. We verified our complete authentication and authorization pipeline through a repeatable Postman demonstration that includes both happy paths and deliberate security rejection cases [9]. Finally, we executed our Definition-of-Done (DoD) checks, completed a Sprint 2 Retrospective, and logged our technical debt as actionable backlog items for Sprint 3 [4, 9].

## What We Learned

- Why displaying deliberate security rejection cases (HTTP 401 and 403) is the only true proof that an API's authorization model is secure [9].
- The critical necessity of moving hardcoded JWT signing keys out of tracked repository files (`appsettings.json`) before production deployment [4].
- How to formulate a structured retrospective that produces a concrete, actionable improvement goal for the upcoming sprint [9].

## Tasks We Completed

### Task 1: Repeatable Postman Demo (with Deliberate Rejections)

We designed and executed a complete, automated Postman test collection (`Sprint2_Day5_Demo.json`) to walk through our authentication, RBAC, and ownership-check pipelines [9]:

1. **User Registration (Transaction Check):** Sent a `POST` request to `/api/auth/register` to create a new Patient account [2]. Confirmed that both `AspNetUsers` and `Patients` tables populated successfully in a single database transaction [5].
2. **User Login (Claims Check):** Logged in as the registered Patient [2]. Decoded the returned JWT and verified that both the role `Patient` and the custom claim `PatientId` were successfully embedded [5].
3. **Authorized Retrieval (Happy Path - 200 OK):** Sent a `GET` request to `/api/patients/ {id}` using the Patient's token [2]. The server successfully returned their matching profile with a `200 OK`.
4. **Admin Route Block (Rejection Case 1 - 403 Forbidden):** Attempted to create a new patient profile (`POST /api/patients`) using the Patient's token [9]. The request was rejected with **HTTP 403 Forbidden** [6].
5. **IDOR Cross-Patient Block (Rejection Case 2 - 403 Forbidden):** Attempted to query Patient B's vitals (`GET /api/patients/2/vitals`) using Patient A's token [9]. The controller compared the token's `PatientId` claim with the path parameter, detected the mismatch, and rejected the request with **HTTP 403 Forbidden** [6, 7].

---

### Task 2 & 3: Definition-of-Done (DoD) Audit & Sprint 3 Backlog Logging

We audited our Sprint 2 deliverables against the program's strict Definition-of-Done criteria [9]. All development tasks are 100% completed, and outstanding security debts have been successfully logged as prioritized items for Sprint 3 [4, 9]:

#### Definition-of-Done (DoD) Checklist

- [x] Correct HTTP Status Codes returned across all authentication and access-control endpoints [9].
- [x] All core endpoints protected by explicit RBAC and stateless ownership checks [6, 7].
- [x] Custom `RequestTimingMiddleware` logging request duration globally [8].
- [x] Zero unhandled exceptions (covered by our global exception handler) [2].
- [x] Postman collection runner successfully completes all happy and error test paths [9].

#### Logged Sprint 3 Backlog Items

- **P0 - Move JWT Signing Key to User Secrets:** Storing the JWT signing key in `appsettings.json` is a security risk [4]. We will migrate it to User Secrets (Local) and Environment Variables (Production) [4, 6].
- **P0 - Restrict Public Role Selection:** Currently, a public register request can request any role (including Admin) [5]. We must restrict public registration to the `Patient` role by default, making Admin/Doctor role assignment administrative-only [6].

---

### Task 4: Sprint 2 Retrospective (Start, Stop, Keep Doing)

We conducted a Sprint 2 Retrospective to identify operational strengths and areas of technical improvement [9]:

- **What Went Well:**
  - Implementing a transaction-backed registration flow guarantees database consistency [5].
  - The custom `PatientId` claim allows for highly efficient, stateless ownership checks without redundant database lookups [5].
  - The `RequestTimingMiddleware` provides excellent performance diagnostics globally without bloating individual controllers [8].
- **What Should Improve:**
  - We must avoid committing plain-text secrets and JWT keys to source control [4, 6].
  - We should start configuring our Postman test assertions earlier in the sprint rather than leaving them until Day 5 [6].
- **Concrete Action for Sprint 3:**
  - *"We will configure Microsoft User Secrets for local development on Day 1 of Sprint 3 and move all cryptographic keys out of `appsettings.json` before writing any new business logic."* [4, 6]

---

### Task 5: Assemble Sprint 2 Summary

All technical evidence, including our relational schema, custom middleware logging output, passing automated tests, and the repeatable Postman collection, has been successfully reviewed, committed, and merged into `main` [8, 9].

## Files Related to Day 5

- `Sprint2_Day5_Demo.postman_collection.json` (Repeatable test runner) [9]
- `docs/WEEK7_IMPLEMENTATION.md` (Notion Board & Retrospective summary) [9]

## Day Result

Sprint 2 is officially closed [9]. Our authentication and authorization models are fully robust, secure, and validated against unauthorized access and IDOR vulnerabilities [6, 7]. The project is approved and prepared for Sprint 3 (Sring 3: Advanced Queries, Redis Caching, and Performance Tuning) [9].
