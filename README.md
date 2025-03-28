﻿# CarbunqleX - SQL Parser and Modeler

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

[Carbunqlex](https://www.nuget.org/packages/Carbunqlex/) can be installed from NuGet. To install using the package manager, use the following command:

```sh
NuGet\Install-Package Carbunqlex
```

## 📖 Documentation

The complete code for the sample used here can be found at [Sample.csproj](https://github.com/mk3008/CarbunqleX/tree/91c752ba382df24238451bad0416bfd4f80bbf82/demo/Sample)

### **1. Parsing a SQL Query**

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

#### 🔍 Expected SQL Output

```sql
SELECT a.table_a_id, a.value FROM table_a AS a
```

This makes it easy to convert between them.

### **2. Modifying the WHERE Clause**

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

CarbunqleX allows you to inject conditions while still maintaining the integrity of your SQL.

### **3. Handling CTEs and Subqueries**

Let's try to insert a `WHERE` condition into a query that contains a **CTE and a subquery**.

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

CarbunqleX will **intelligently place the condition in the deepest related query**. It will also **dynamically merge CTEs**.

### **4. Standardizing filtering**

Next, we will introduce a more advanced use case that dynamically filters data based on user permissions. We define the areas that users can access as reusable functions.

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

By making filtering a function, we can express queries that are **highly maintainable and versatile**.

### **5. Modify columns** 

This feature is useful for enforcing constraints such as **closing date control in accounting** by ensuring a column value does not fall below a specified threshold.  

```csharp
var query = QueryAstParser.Parse("SELECT s.sale_date, s.sales_amount FROM sales AS s");

// Ensure sale_date is at least '2024-01-01'
query.ModifyColumn("sale_date", c => c.Greatest(new DateTime(2024, 1, 1)));

Console.WriteLine(query.ToSql());
```

#### 🔍 Expected SQL Output  

```sql
SELECT GREATEST(s.sale_date, '2024-01-01 00:00:00') AS sale_date, s.sales_amount 
FROM sales AS s;
```

In this way, column processing can be made common.

### **6. Manage UNION queries as modular components**

This approach allows you to manage `UNION` queries as separate components, improving maintainability and modularity.

```csharp
var query1 = QueryAstParser.Parse("SELECT id FROM table_a");
var query2 = QueryAstParser.Parse("SELECT id FROM table_b");

// Create a UNION ALL query and wrap it as a distinct subquery
var distinctQuery = query1.UnionAll(query2).ToSubQuery().Distinct();

Console.WriteLine(distinctQuery.ToSql());
```

#### 🔍 Expected SQL Output  

```sql
SELECT DISTINCT * FROM (SELECT id FROM table_a UNION ALL SELECT id FROM table_b) AS d;
```

Now managing huge union queries is not scary.

### **7. Filtering using outer joins**

You can also dynamically insert outer join conditions to perform filtering.

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
  SELECT orders.region,  
         orders.product,  
         SUM(orders.quantity) AS product_units,  
         SUM(orders.amount) AS product_sales  
  FROM orders   
  WHERE orders.region IN (SELECT x.region FROM top_regions x)  
  GROUP BY orders.region, orders.product  
""");

// Set isCurrentOnly: false to use the join as a filter condition  
query.From("region", isCurrentOnly: false, static from =>
{
    from.LeftJoin("top_regions", "tp");
    from.EditQuery(q =>
    {
        q.Where("tp.region is null");
    });
});

// left join top_regions as tp on orders.region = tp.region where tp.region is null  
var expected = "with regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders left join top_regions as tp on orders.region = tp.region where tp.region is null group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales from orders where orders.region in (select x.region from top_regions as x) group by orders.region, orders.product";
```

### **8. Create a table from a select query**

You can convert a select query into a `CREATE TABLE` statement. This is very useful when you want to create a new table based on the results of a query.

```csharp
var query = QueryAstParser.Parse("SELECT a.table_a_id, 1 AS value FROM table_a AS a");

// Generate a CREATE TABLE query, specifying that the table is temporary
var createTableQuery = query.ToCreateTableQuery("table_b", isTemporary: true);

// Print the generated SQL query
Console.WriteLine(createTableQuery.ToSql());
```

#### 🔍 Expected SQL output

```sql
CREATE TEMPORARY TABLE table_b AS SELECT a.table_a_id, 1 AS value FROM table_a AS a;
```

### 9. Manage Update Queries with Select Queries

The effects of insert, update, and delete queries can only be seen after they are executed, making it difficult to preview the expected results in advance.

CarbunqleX allows you to express these queries as select queries. Select queries simplify the debugging and validation process by allowing you to preview changes without actually modifying tables.

```csharp
var query = QueryAstParser.Parse("SELECT a.table_a_id, 1 AS value FROM table_a AS a");

// Generate an INSERT INTO query for table_b with a RETURNING clause
var insertTableQuery = query.ToInsertQuery("table_b", hasReturning: true);

// Print the generated SQL query
Console.WriteLine(insertTableQuery.ToSql());
```

#### 🔍 Expected SQL output

```sql
INSERT INTO table_b(table_a_id, value)
SELECT a.table_a_id, 1 AS value FROM table_a AS a
RETURNING *;
```

The above example demonstrates how to express an insert query using a select query, but similar techniques can also be applied to update and delete queries.

For update queries:

```csharp
var query = QueryAstParser.Parse("SELECT a.table_a_id, 1 AS value FROM table_a AS a");

var updateQuery = query.ToUpdateQuery("table_b", new[] { "table_a_id" });

var expected = "UPDATE table_b SET value = q.value FROM (SELECT a.table_a_id, 1 AS value FROM table_a AS a) AS q WHERE table_b.table_a_id = q.table_a_id";
```

For delete queries:

```csharp
var query = QueryAstParser.Parse("SELECT a.table_a_id, 1 AS value FROM table_a AS a");

var deleteQuery = query.ToDeleteQuery("table_b", new[] { "table_a_id" });

var expected = "DELETE FROM table_b WHERE table_b.table_a_id IN (SELECT q.table_a_id FROM (SELECT a.table_a_id, 1 AS value FROM table_a AS a) AS q)";
```

### 10. Converting a Query to Return JSON (Postgres-only) ver 0.0.3 or later

In PostgreSQL, you can transform a query to return JSON using functions like `json_build_object` and `row_to_json`.

```csharp
var query = QueryAstParser.Parse("""
    SELECT 
        posts.post_id, 
        posts.title, 
        posts.content, 
        posts.created_at, 
        users.user_id, 
        users.name AS user_name, 
        blogs.blog_id, 
        blogs.name AS blog_name, 
        organizations.organization_id, 
        organizations.name AS organization_name
    FROM posts
    INNER JOIN users ON posts.user_id = users.user_id
    INNER JOIN blogs ON posts.blog_id = blogs.blog_id
    INNER JOIN organizations ON blogs.organization_id = organizations.organization_id
""");

query.Where("post_id", action: x => x.Equal(":post_id"))
    .NormalizeSelectClause()
    .ToPostgresJsonQuery(x =>
    {
        return x.Serialize(datasource: "posts", jsonKey: "post", parent: static x =>
        {
            return x.Serialize(datasource: "users", jsonKey: "user")
                .Serialize(datasource: "blogs", jsonKey: "blog", parent: static x =>
                {
                    return x.Serialize(datasource: "organizations", jsonKey: "organization");
                });
        });
    });
```

#### 🔍 Expected SQL output

```sql
WITH
    __json AS (
        SELECT
            posts.post_id AS posts__post_id,
            posts.title AS posts__title,
            posts.content AS posts__content,
            posts.created_at AS posts__created_at,
            users.user_id AS users__user_id,
            users.name AS users__user_name,
            blogs.blog_id AS blogs__blog_id,
            blogs.name AS blogs__blog_name,
            organizations.organization_id AS organizations__organization_id,
            organizations.name AS organizations__organization_name
        FROM
            posts
            INNER JOIN users ON posts.user_id = users.user_id
            INNER JOIN blogs ON posts.blog_id = blogs.blog_id
            INNER JOIN organizations ON blogs.organization_id = organizations.organization_id
        WHERE
            posts.post_id = :post_id
    )
SELECT
    ROW_TO_JSON(d)
FROM
    (
        SELECT
            JSON_BUILD_OBJECT(
                'post_id', __json.posts__post_id,
                'title', __json.posts__title,
                'content', __json.posts__content,
                'created_at', __json.posts__created_at,
                'user', JSON_BUILD_OBJECT(
                    'user_id', __json.users__user_id,
                    'user_name', __json.users__user_name
                ),
                'blog', JSON_BUILD_OBJECT(
                    'blog_id', __json.blogs__blog_id,
                    'blog_name', __json.blogs__blog_name,
                    'organization', JSON_BUILD_OBJECT(
                        'organization_id', __json.organizations__organization_id,
                        'organization_name', __json.organizations__organization_name
                    )
                )
            ) AS "post"
        FROM
            __json
    ) AS d
LIMIT
    1
```

#### 🔍 Query Output

```json
{
    "post": {
        "post_id": 1,
        "title": "Understanding AI",
        "content": "This is a post about AI.",
        "created_at": "2025-02-18T20:25:21.974106",
        "user": {
            "user_id": 1,
            "user_name": "Alice"
        },
        "blog": {
            "blog_id": 1,
            "blog_name": "AI Insights",
            "organization": {
                "organization_id": 1,
                "organization_name": "Tech Corp"
            }
        }
    }
}
```

### 11. Converting a Query to Return JSON Array (Postgres-only) ver 0.0.3 or later

You can also get a JSON array. Since the results are aggregated on the DB server, there is no need to loop through the DataReader. There is also no need to issue multiple queries.

```csharp
var query = QueryAstParser.Parse("""
    SELECT 
        posts.post_id, 
        posts.title, 
        posts.content, 
        posts.created_at, 
        users.user_id, 
        users.name AS user_name, 
        blogs.blog_id, 
        blogs.name AS blog_name, 
        organizations.organization_id, 
        organizations.name AS organization_name
    FROM posts
    INNER JOIN users ON posts.user_id = users.user_id
    INNER JOIN blogs ON posts.blog_id = blogs.blog_id
    INNER JOIN organizations ON blogs.organization_id = organizations.organization_id
""");

query.Where("user_id", action: x => x.Equal(":user_id"))
    .NormalizeSelectClause()
    .ToPostgresJsonQuery(x =>
    {
        return x.Serialize(datasource: "users", jsonKey: "user", parent: static x =>
        {
            return x.SerializeArray(datasource: "posts", jsonKey: "posts", parent: static x =>
            {
                return x.Serialize(datasource: "blogs", jsonKey: "blog", parent: static x =>
                {
                    return x.Serialize(datasource: "organizations", jsonKey: "organization");
                });
            });
        });
    });
```

#### 🔍 Expected SQL output

```sql
WITH
    __json AS (
        SELECT
            posts.post_id AS posts__post_id,
            posts.title AS posts__title,
            posts.content AS posts__content,
            posts.created_at AS posts__created_at,
            users.user_id AS users__user_id,
            users.name AS users__user_name,
            blogs.blog_id AS blogs__blog_id,
            blogs.name AS blogs__blog_name,
            organizations.organization_id AS organizations__organization_id,
            organizations.name AS organizations__organization_name
        FROM
            posts
            INNER JOIN users ON posts.user_id = users.user_id
            INNER JOIN blogs ON posts.blog_id = blogs.blog_id
            INNER JOIN organizations ON blogs.organization_id = organizations.organization_id
        WHERE
            users.user_id = :user_id
    ),
    __json_posts AS (
        SELECT
            __json.users__user_id,
            __json.users__user_name,
            JSON_AGG(JSON_BUILD_OBJECT('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'blog', JSON_BUILD_OBJECT('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', JSON_BUILD_OBJECT('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)))) AS users__posts
        FROM
            __json
        GROUP BY
            __json.users__user_id,
            __json.users__user_name
    )
SELECT
    ROW_TO_JSON(d)
FROM
    (
        SELECT
            JSON_BUILD_OBJECT(
                'user_id', __json_posts.users__user_id,
                'user_name', __json_posts.users__user_name,
                'posts', __json_posts.users__posts
            ) AS "user"
        FROM
            __json_posts
    ) AS d
LIMIT
    1
```

#### 🔍 Query Output

```json
{
  "user": {
    "user_id": 1,
    "user_name": "Alice",
    "posts": [
      {
        "post_id": 9,
        "title": "Understanding AI",
        "content": "This is a post about AI.",
        "created_at": "2025-02-18T20:25:21.974106",
        "blog": {
          "blog_id": 1,
          "blog_name": "AI Insights",
          "organization": {
            "organization_id": 1,
            "organization_name": "Tech Corp"
          }
        }
      },
      {
        "post_id": 11,
        "title": "Understanding AI",
        "content": "This is a post about AI.",
        "created_at": "2025-03-07T17:53:49.168646",
        "blog": {
          "blog_id": 1,
          "blog_name": "AI Insights",
          "organization": {
            "organization_id": 1,
            "organization_name": "Tech Corp"
          }
        }
      }
    ]
  }
}
```

### To improve maintainability

Defining parent-child relationships using local functions makes them easier to maintain.

![image](https://github.com/user-attachments/assets/1a06a0d3-4e05-4c38-bd53-7297fb51131b)

## 📌 Conclusion

CarbunqleX makes raw SQL **more maintainable, reusable, and dynamically modifiable** without sacrificing performance. Its AST-based transformations provide a powerful way to manipulate queries at scale, making it an essential tool for advanced SQL users.
