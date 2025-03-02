# CarbunqleX - SQL Parser and Modeler

![GitHub](https://img.shields.io/github/license/mk3008/Carbunqlex)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/mk3008/Carbunqlex)
![Github Last commit](https://img.shields.io/github/last-commit/mk3008/Carbunqlex)  
[![Carbunqlex](https://img.shields.io/nuget/v/Carbunqlex.svg)](https://www.nuget.org/packages/Carbunqlex/)
[![Carbunqlex](https://img.shields.io/nuget/dt/Carbunqlex.svg)](https://www.nuget.org/packages/Carbunqlex/)

## 🚀 Overview

**CarbunqleX** enhances the reusability and maintainability of raw SQL queries by deeply analyzing their Abstract Syntax Tree (AST). This allows for powerful transformations while preserving query semantics. With CarbunqleX, you can:

- Modify selection columns
- Inject `JOIN` and `WHERE` conditions dynamically
- Transform queries into different SQL statements (`CREATE TABLE AS`, `INSERT INTO`, `UPDATE`, `DELETE`)

## 💡 Key Features

### Advanced CTE Handling

CarbunqleX offers **flexible Common Table Expression (CTE) processing**. Traditionally, CTEs exist only within the `WITH` clause and cannot be referenced in `WHERE` or `JOIN` conditions. However, CarbunqleX detects existing CTEs and **lifts them to the top level**, making them accessible in places where they would otherwise be restricted. This enables highly flexible query modifications.

### Intelligent Search Condition Injection

Unlike conventional SQL libraries, CarbunqleX automatically determines the most appropriate insertion point for search conditions, even within complex queries involving subqueries and CTEs. This ensures optimal filtering while preserving query integrity. For example, when dealing with `GROUP BY`, conditions are inserted **before** aggregation to ensure correctness.

### Lightweight and Easy to Use

- **Minimal dependencies** – Works directly with raw SQL
- **No special setup or DBMS required** – Purely operates on query strings
- **Seamless ORM integration** – Works alongside existing ORM frameworks

## 📦 Installation

To install [Carbunqlex](https://www.nuget.org/packages/Carbunqlex/), use the following command:

```sh
PM> NuGet\Install-Package Carbunqlex
```

## 📖 Documentation

### 1️⃣ Parsing a SQL Query

Let's start by parsing a simple SQL query into an AST using `QueryAstParser.Parse`. We will then convert it back to SQL with `ToSql` and inspect its structure using `ToTreeString`.

```csharp
using Carbunqlex;
using Carbunqlex.Parsing;

var sql = "SELECT a.table_a_id, a.value FROM table_a AS a";
var query = QueryAstParser.Parse(sql);

// Convert back to SQL
Console.WriteLine("* SQL");
Console.WriteLine(query.ToSql());

// View AST structure (useful for debugging)
Console.WriteLine("* AST");
Console.WriteLine(query.ToTreeString());
```

This basic example demonstrates how CarbunqleX converts SQL into a structured AST representation, making it easier to analyze and manipulate queries programmatically.

### 2️⃣ Modifying the WHERE Clause

Now, let's modify the query by injecting a `WHERE` condition.

```csharp
var query = QueryAstParser.Parse("SELECT a.table_a_id, a.value FROM table_a AS a");

// Inject a filter condition
query.Where("value", w => w.Equal(1));

Console.WriteLine(query.ToSql());
```

#### 🔍 Expected SQL Output

```sql
SELECT a.table_a_id, a.value
FROM table_a AS a
WHERE a.value = 1;
```

CarbunqleX automatically determines where to insert the condition while maintaining SQL integrity.

### 3️⃣ Handling CTEs and Subqueries

Let's take it a step further by injecting a `WHERE` condition into a query that includes **CTEs and subqueries**.

```csharp
var query = QueryAstParser.Parse("""
    WITH regional_sales AS (
        SELECT orders.region, SUM(orders.amount) AS total_sales
        FROM orders
        GROUP BY orders.region
    ), top_regions AS (
        SELECT rs.region
        FROM regional_sales rs
        WHERE rs.total_sales > (SELECT SUM(x.total_sales)/10 FROM regional_sales x)
    )
    SELECT orders.region, orders.product, SUM(orders.quantity) AS product_units, SUM(orders.amount) AS product_sales
    FROM orders
    WHERE orders.region IN (SELECT x.region FROM top_regions x)
    GROUP BY orders.region, orders.product
""");

query.Where("region", w => w.Equal("'east'"));
```

#### 🔍 Modified SQL Output

```sql
WITH regional_sales AS (
    SELECT orders.region, SUM(orders.amount) AS total_sales
    FROM orders
    WHERE orders.region = 'east'
    GROUP BY orders.region
),
 top_regions AS (
    SELECT rs.region
    FROM regional_sales rs
    WHERE rs.total_sales > (SELECT SUM(x.total_sales)/10 FROM regional_sales x)
)
SELECT orders.region, orders.product, SUM(orders.quantity) AS product_units, SUM(orders.amount) AS product_sales
FROM orders
WHERE orders.region IN (SELECT x.region FROM top_regions x)
GROUP BY orders.region, orders.product;
```

CarbunqleX **intelligently places the condition in the deepest relevant query**, ensuring correctness.

### 4️⃣ Advanced Filtering

Now, let's introduce a **more advanced use case** where we dynamically filter data based on user permissions. We'll define a reusable function to retrieve regions a user has access to and use it in a filtering condition.

#### 🔧 Define a Subquery Function

```csharp
private QueryNode BuildRegionScalarQueryByUser(int userId)
{
    return QueryAstParser.Parse("""
        WITH user_permissions AS (
            SELECT rrp.region
            FROM region_reference_permission rrp
            WHERE rrp.user_id = :user_id
        )
        SELECT up.region FROM user_permissions up
    """)
    .AddParameter(":user_id", userId);
}
```

#### 🔍 Apply the Function in a Query

```csharp
var query = QueryAstParser.Parse("...");
query.Where("region", w => w.Exists(BuildRegionScalarQueryByUser(1)));
```

#### 🔍 Expected SQL Output

```sql
WITH user_permissions AS (
    SELECT rrp.region
    FROM region_reference_permission AS rrp
    WHERE rrp.user_id = :user_id
), regional_sales AS (
    SELECT orders.region, SUM(orders.amount) AS total_sales
    FROM orders
    WHERE EXISTS (
        SELECT * 
        FROM (SELECT up.region FROM user_permissions AS up) AS x 
        WHERE orders.region = x.region
    )
    GROUP BY orders.region
), top_regions AS (
    SELECT rs.region
    FROM regional_sales AS rs
    WHERE rs.total_sales > (SELECT SUM(x.total_sales) / 10 FROM regional_sales AS x)
)
SELECT orders.region, orders.product, SUM(orders.quantity) AS product_units, SUM(orders.amount) AS product_sales
FROM orders
WHERE orders.region IN (SELECT x.region FROM top_regions AS x)
GROUP BY orders.region, orders.product
```

By dynamically injecting permission-based filtering, we can ensure **secure and flexible query customization**.

## 📌 Conclusion

CarbunqleX makes raw SQL **more maintainable, reusable, and dynamically modifiable** without sacrificing performance. Its AST-based transformations provide a powerful way to manipulate queries at scale, making it an essential tool for advanced SQL users.
