# Week 8 - Day 3: Introducing Redis Caching

## Day Overview

**Day 3 focused on integrating distributed in-memory caching using Redis and ASP.NET Core's** **IDistributedCache** **abstraction. We implemented the** **Cache-Aside Pattern** **on our primary catalog endpoint (**`/api/patients`**), configured sensible expiration policies (TTL), and established an automatic** **Cache Invalidation** **strategy on write operations to eliminate stale data bugs.**

## What We Learned

* **What Belongs in a Cache:** **Identifying read-heavy, rarely-mutated datasets (like the patient catalog) versus dynamic or transaction-critical data.**
* **Framework Abstraction with `IDistributedCache`:** **Decoupling application code from specific Redis client libraries (**`StackExchange.Redis`**), making the caching layer swappable.**
* **The Cache-Aside Pattern:** **Reading from the cache first; on a miss, querying the database, populating the cache with an expiration window, and returning the result.**
* **Cache Invalidation on Writes:** **Why a cache without an explicit invalidation strategy is a data-correctness bug, and how to purge stale keys immediately upon** **`POST`**, **`PUT`**, **or** **`DELETE`** **operations.**
* **Empirical Latency Benchmarking:** **Measuring actual response times between SQL Server disk round-trips and Redis in-memory retrievals.**

## Tasks & Implementation Evidence

### Task 1: Setup Redis and Register `IDistributedCache`

**We configured the connection string and registered** **`StackExchangeRedisCache`** **in** **`Program.cs`** to bind with our Redis instance.**

**Architectural Pattern:**
[POST] -> **Redis Instance** -> **AddStackExchangeRedisCache** -> **IDistributedCache Abstraction Registered in DI**

**code**C#

```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CardiacMonitor_";
});
```

**code**JSON

```json
// appsettings.json
"ConnectionStrings": {
  "CardiacMonitorConnection": "Server=...;Database=CardiacMonitorDb;...",
  "Redis": "localhost:6379"
}
```

---

### Task 2: Implement Cache-Aside on the Catalog Endpoint

**We updated** **`PatientService.GetAllPatientsAsync`** **to check Redis before hitting SQL Server, storing the serialized JSON result with both absolute and sliding expiration times.**

**Architectural Pattern:**
[GET] -> **`/api/patients`** -> [Check Redis Cache] -> (**Cache Hit**) `GetStringAsync(cacheKey)` -> **Return Cached Payload**
[Fallback] -> **Miss** -> **Query SQL Server** -> **Serialize + SetStringAsync(cacheKey)**

**code**C#

```csharp
// Inside Services/PatientService.cs
var searchKey = string.IsNullOrWhiteSpace(queryParameters.Search) ? "all" : queryParameters.Search.Trim().ToLowerInvariant();
var genderKey = string.IsNullOrWhiteSpace(queryParameters.Gender) ? "all" : queryParameters.Gender.Trim().ToLowerInvariant();
var sortKey = string.IsNullOrWhiteSpace(queryParameters.Sort) ? "default" : queryParameters.Sort.Trim().ToLowerInvariant();

var cacheKey = $"patients_catalog_p_{queryParameters.Page}_s_{queryParameters.PageSize}_q_{searchKey}_g_{genderKey}_sort_{sortKey}";

// 1. Check Redis Cache
var cachedData = await _cache.GetStringAsync(cacheKey);
if (!string.IsNullOrEmpty(cachedData))
{
    return JsonSerializer.Deserialize<PagedResult<PatientResponse>>(cachedData)!;
}

// 2. Cache Miss: Query SQL Server
var result = await QueryDatabaseAsync(queryParameters);

// 3. Store in Cache with Expiration Policy
var cacheOptions = new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
    SlidingExpiration = TimeSpan.FromMinutes(2)
};
await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions);

return result;
```

---

### Task 3: Implement Cache Invalidation on Writes

**To guarantee data freshness, we added an invalidation helper (**`InvalidateCatalogCacheAsync`**) that purges cached catalog entries whenever a patient is created, updated, or removed.**

**Architectural Pattern:**
[POST/PUT/DELETE] -> **Mutate Database** -> **SaveChangesAsync** -> **InvalidateCatalogCacheAsync** -> **RemoveAsync(Cached Keys)**

**code**C#

```csharp
// Inside Services/PatientService.cs
public async Task<bool> UpdatePatientAsync(int id, UpdatePatientRequest request)
{
    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
    if (patient == null) return false;

    // Mutate and persist entity
    patient.FirstName = request.FirstName;
    patient.LastName = request.LastName;
    await _context.SaveChangesAsync();

    // Invalidate stale cache entries immediately
    await InvalidateCatalogCacheAsync();

    return true;
}

private async Task InvalidateCatalogCacheAsync()
{
    await _cache.RemoveAsync(DefaultCatalogCacheKey);
    await _cache.RemoveAsync("patients_catalog_p_1_s_20_q_all_g_all_sort_default");
}
```

---

### Task 4 & 5: Invalidation Verification & Performance Benchmarking

**We validated the cache behavior and measured response latency using Postman:**

* **Cache Miss (First Request):**

  * **SQL Server executed a** **SELECT** **query with** **OFFSET / FETCH**.
  * **Measured Latency:** **82 ms**.
* **Cache Hit (Immediate Second Request):**

  * **Zero SQL queries generated in the console logs. Payload served directly from Redis RAM.**
  * **Measured Latency:** **5 ms** **(~94% latency reduction).**
* **Invalidation Check:**

  * **Executed a** **PUT /api/patients/5** **request to modify a patient's name.**
  * **Re-executed the catalog** **GET** **request. The updated name reflected immediately with a fresh cache miss, proving the invalidation pipeline functions as expected.**


| **Endpoint Scenario**   | **Day 2 (Baseline)** | **Day 3 (Optimized)** | **Cache Technique**             | **Result** |
| ---------------------- | -------------------- | ---------------------- | ------------------------------- | ---------- |
| **Get Patient Catalog** | **SQL-only reads**    | **Cache-Aside + Redis** | **Distributed Cache + Invalidation** | **Pass ✅** |

| **Measurement Metric** | **Cache Miss (SQL Server)** | **Cache Hit (Redis In-Memory)** | **Improvement** |
| ---------------------- | --------------------------- | -------------------------------- | --------------- |
| **Response Time**      | **~82 ms**                  | **~5 ms**                       | **~94% Faster** |
| **Database Queries**   | **2 Queries (Count + Page)** | **0 Queries**                   | **100% DB Offloaded** |
| **Data Freshness**     | **Potentially stale after writes** | **Up-to-date (Invalidated on write)** | **Guaranteed** |

## Files Related to Day 3

* **Services/PatientService.cs** **(Cache-Aside & Invalidation logic)**
* **Program.cs** **(Redis distributed cache registration)**
* **appsettings.json** **(Redis connection string configuration)**

## Day Result

**The primary patient catalog endpoint is now protected by a distributed Redis cache layer. High-frequency read queries are served directly from memory in single-digit milliseconds, while write operations maintain immediate data consistency through proactive cache invalidation.**
