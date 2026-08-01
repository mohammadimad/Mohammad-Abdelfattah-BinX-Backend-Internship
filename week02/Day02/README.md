# 📂 Day 02: Advanced LINQ & Deferred Execution

## 📝 Objective
The primary focus of Day 2 was to master advanced data querying and reshaping in C# using **Language Integrated Query (LINQ)**. This lab explores the architectural difference between Deferred and Immediate execution, aggregates datasets using `GroupBy`, combines disparate tables using relational `Join`, and flattens nested hierarchies with `SelectMany`.

---

## 🛠️ Completed Lab Tasks

1. **Relational Dataset Simulation:**
   - Populated two related in-memory collections representing a database schema: `List<Customer>` and `List<Order>` (linked via a `CustomerId` Foreign Key relationship) with 6+ records each.

2. **Data Aggregation (GroupBy):**
   - Engineered a query to group orders by their unique customer key and utilized the `.Sum()` operator to calculate the total sales amount per customer.
   - Evaluated the performance benefits of grouping in memory.

3. **Relational Inner Join (Join):**
   - Performed a fluent `Join` operation between the Customers and Orders collections based on their matching primary/foreign keys.
   - Projected the result into a clean, lightweight anonymous type containing only the required fields (`Name` and `Amount`).

4. **Hierarchical Flattening (SelectMany):**
   - Utilized `.SelectMany()` to query a nested collection of phone numbers (`List<string> PhoneNumbers`) inside the `Customer` class.
   - Flattened the nested list of lists into a single, contiguous, flat sequence of phone numbers (`IEnumerable<string>`).

5. **Deferred Execution Verification Lab:**
   - Created a defensive LINQ query against a source collection.
   - Modified the underlying source list (adding a new compliant element) **after** the query definition but **before** its evaluation.
   - Demonstrated that deferred queries evaluate dynamically only during enumeration (e.g., inside a `foreach` loop), reflecting the change, whereas immediate queries (materialized via `.ToList()`) remain frozen in state.
