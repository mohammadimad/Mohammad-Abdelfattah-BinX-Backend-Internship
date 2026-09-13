# Sprint 3 Close-Out: Advanced Queries & Performance

## 1. Demo Script & Measured Performance Evidence

**This demonstration presents empirical, measurable data comparing the baseline system against optimized states across three key dimensions:**

### A. Database Round-Trips (N+1 Elimination)

* **Endpoint:** **GET /api/patients/fixed-performance** vs. **GET /api/test-n-plus-one**
* **Before (Baseline):** 56 discrete SQL **SELECT** statements (1 query for patient batch + 55 queries for telemetry records in a loop).
* **After (Eager Loading & Projection):** **1 single SQL query** utilizing an inner/left join and column projection via **.Select()**.
* **Impact:** **98.2% reduction** in network round-trips and database connection pool contention.

### B. Response Latency (Redis Cache-Aside)

* **Endpoint:** **GET /api/patients?page=1&pageSize=10**
* **Cache Miss (Direct Database Query):** **\~82 ms** (involves disk I/O, paging offsets, and serialization).
* **Cache Hit (In-Memory Redis):** **\~5 ms** (payload retrieved directly from RAM via **IDistributedCache**).
* **Impact:** **\~94% decrease in latency**, with 100% database offload during cached windows.
* **Invalidation Check:** Triggering **PUT /api/patients/5** immediately evicts the stale cache entry, ensuring subsequent reads serve fresh data without waiting for the 10-minute TTL to expire.

### C. Execution Plan Analysis (Database Indexing)

* **Query:** **SELECT ... FROM VitalSigns WHERE PatientId = @id ORDER BY RecordedAt DESC**
* **Before Index (**PK\_VitalSigns** Scan):** Full table clustered scan with an explicit in-memory **Sort** operator consuming \~70% of total query execution cost.
* **After Index (**IX\_VitalSigns\_PatientId\_RecordedAt**):** Switched to an optimal **Index Seek**; the **Sort** operator was **completely eliminated (0% cost)** because data is physically structured in the required order.
* **Impact:** Relative query cost dropped from **78% down to 22%** in side-by-side batch profiling.

---

## 2. Sprint 3 Backlog Audit & Definition-of-Done (DoD)


| **Backlog Task**                                 | **Target Metric**                              | **Status**    | **Evidence**                                                                                                              |
| ------------------------------------------------ | ---------------------------------------------- | ------------- | ------------------------------------------------------------------------------------------------------------------------- |
| **TSK-301: N+1 Diagnosis & Elimination**         | **Reduce query count from 50+ to ≤ 2**        | **Completed** | **Reduced from 56 queries to 1 query using**.Include**and**.Select**.**                                                   |
| **TSK-302: Redis Cache-Aside Implementation**    | **Sub-10ms response time on cached reads**     | **Completed** | **Response latency reduced from \~82ms to \~5ms; invalidation verified on writes.**                                       |
| **TSK-303: Composite Database Indexing**         | **Eliminate table scans and sort operators**   | **Completed** | **Added**IX\_VitalSigns\_PatientId\_RecordedAt**,**IX\_Patients\_Gender\_LastName**, and unique appointment constraint.** |
| **TSK-304: Performance Profiling Documentation** | **Empirical Before/After comparison recorded** | **Completed** | **Full execution plan metrics and query count logs documented.**                                                          |

---

## 3. Tagged Backlog Items for Sprint 4

**Any optimization opportunities outside the immediate critical path of Sprint 3 have been logged as prioritized backlog items for **Sprint 4 (Testing, Documentation & Final Deployment)**:**

* **[PERF-401] Automated Query-Count Regression Testing** (Priority: High):

* **Description:** Implement xUnit tests using EF Core event counters to assert that the patient catalog query count remains strictly at 1, preventing future code changes from introducing silent N+1 regressions.
* **[PERF-402] Response Compression Middleware** (Priority: Medium):

  * **Description:** Enable Gstandard Gzip/Brotli compression for payload responses exceeding 50KB to reduce bandwidth consumption for large telemetry queries.
* **[PERF-403] Redis Tag-Based Multi-Key Invalidation** (Priority: Low):

  * **Description:** Refactor catalog cache keys to use a pattern or tag-based invalidator to purge all filtered query permutations simultaneously upon entity mutations.

---

## 4. Sprint 3 Retrospective

### What Went Well

* **Evidence-Driven Engineering:** Optimization decisions were justified using concrete SQL Server execution plans and network benchmarks rather than intuition.
* **Effective Query Projection:** Using **.Select()** for summary views proved leaner than eager loading, transferring only essential columns over the wire.
* **Clean Architecture Preservation:** Moved all performance querying logic into **PatientService** while keeping controllers thin and maintainable.

### What Should Improve

* **Local Caching Environment Dependency:** Setting up Redis locally without containerization caused initial setup friction; containerized setups via Docker should be standardized earlier.
* **Cache Key Granularity:** Managing separate cache keys for varying pagination and filter combinations introduces complexity in invalidation tracking.

### One Concrete Action for Sprint 4

> **"Before final deployment, implement an automated integration test verifying that the primary catalog endpoint generates exactly one database query, establishing a permanent test safeguard against performance degradation."**
>
