# 📂 Day 05: Securing the API (Rate Limiting, CORS & Security Headers)

## 📝 Objective

The primary focus of Day 5 was **API Hardening**. This lab wraps up Week 4 by applying non-optional, production-grade security layers to protect the API from common vulnerabilities such as Brute-Force attacks, Cross-Site Scripting (XSS), and SQL Injection.

---

## 🧠 Architectural Security Decisions

### 1. Throttling Abuse (Rate Limiting)

- **Concept:** Limiting the number of requests a single client can make within a specific time window.
- **Application:** Applied generic rate limiting across the API, with a specifically strict policy applied to the **Login Endpoint**. This is crucial to mitigate bot-driven brute-force attacks and simple Denial-of-Service (DoS) patterns.

### 2. Restricting Origins (CORS Configuration)

- **Concept:** Cross-Origin Resource Sharing (CORS) dictates which external web domains are permitted to interact with the API directly from a browser.
- **Application:** Replaced the permissive "allow any origin" (`*`) development wildcard with a strict, **Named CORS Policy**. This ensures that only trusted and verified frontend domains can execute scripts and access authorized endpoints on behalf of the user.

### 3. Baseline Security Headers (HTTPS & HSTS)

- **Concept:** Forcing all client-server communications over encrypted channels.
- **Application:** Configured the pipeline with **HTTPS Redirection** and **HSTS (HTTP Strict Transport Security)** headers. This instructs browsers to inherently refuse any unencrypted (HTTP) connections to the domain, closing off man-in-the-middle (MitM) attack vectors.

### 4. SQL Injection Prevention

- **Concept:** Preventing malicious user input from executing arbitrary database commands.
- **Application:** Verified that Entity Framework Core inherently parameterizes all LINQ queries by default. Ensured the codebase is free of raw, unparameterized string interpolations (e.g., bypassing `FromSqlInterpolated`), guaranteeing absolute safety against SQL injection.

---

## 🛠️ Hands-On Lab Deliverables

The following security hardening tasks were successfully implemented and tested in the pipeline:

- [X]  **Rate Limiting:** Configured and applied strict rate-limiting policies, specifically prioritizing authentication endpoints.
- [X]  **CORS Policy:** Implemented a named CORS policy restricting access to specific frontend origins, successfully verifying the rejection of disallowed domains.
- [X]  **Security Headers:** Activated `UseHttpsRedirection` and `UseHsts` within the middleware pipeline.
- [X]  **SQL Injection Audit:** Reviewed the codebase to confirm zero usage of raw, vulnerable SQL strings.
- [X]  **Postman Verification:** Confirmed standard endpoints are functioning under the new security restrictions without breaking legitimate traffic.

---

*Prepared by **[Your Name]** as part of the BinX Tech Backend Internship Program.*
