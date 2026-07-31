# 📂 Day 03: Async/Await Deep Dive & Concurrency Basics

## 📝 Objective
The primary focus of Day 3 was to master the **Task-Based Asynchronous Pattern (TAP)** and thread-pool optimization in .NET 8. This lab demonstrates how to write scalable, non-blocking asynchronous APIs, analyze the performance gap between sequential awaits and concurrent execution (`Task.WhenAll`), and implement cooperative cancellation using `CancellationToken`.

---

## 🛠️ Completed Lab Tasks

1. **Multi-Source Asynchronous Simulation:**
   - Designed 3 distinct async methods returning `Task<string>`, simulating data retrieval from three separate external APIs (e.g., Weather, Exchange Rates, and Stock Inventory).
   - Simulating network/database latency using non-blocking delays:
     ```csharp
     await Task.Delay(ms);
     ```

2. **Sequential Execution Analysis (Blocking Scenario):**
   - Measured sequential await performance using `System.Diagnostics.Stopwatch`.
   - Demonstrated that awaiting tasks individually runs them sequentially, forcing a total execution time equal to the sum of all latencies (approximately **6 seconds**).

3. **Concurrent Execution Optimization (Task.WhenAll):**
   - Refactored the execution flow to trigger all three asynchronous operations concurrently in the background.
   - Leveraged `Task.WhenAll` to await all tasks as a single consolidated block.
   - Demonstrated a **50% performance improvement**, reducing the total execution time to only the duration of the longest single task (approximately **3 seconds**).

4. **Cooperative Cancellation:**
   - Added a `CancellationToken` parameter to the longest running async method.
   - Instantiated a `CancellationTokenSource` and configured it to trigger cancellation programmatically (`CancelAfter(1500)`).
   - Wrapped the invocation inside a `try-catch` block targeting **`OperationCanceledException`** to gracefully abort running operations and free up server resources mid-operation.

---

## 📂 Project File Structure
- `Day03.sln` - Visual Studio solution file.
- `Day03.csproj` - C# project configuration.
- `Program.cs` - Asynchronous entry point and concurrent operations logic.
- `README.md` - Day 03 documentation (this file).

---

## ⚙️ Performance Comparison
- **Sequential Awaits:** ~6000ms ($1s + 2s + 3s$)
- **Concurrent Awaits (WhenAll):** ~3000ms ($Max(1s, 2s, 3s)$)
- **Aborted Execution (Cancelled):** Aborted gracefully after ~1500ms.