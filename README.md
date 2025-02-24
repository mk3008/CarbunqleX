# CarbunqleX - SQL Parser and Modeler

**CarbunqleX** dramatically improves the reusability and maintainability of RawSQL.

By deeply analyzing the AST of RawSQL—including subqueries and CTEs—it enables advanced query modifications while preserving semantic integrity. With CarbunqleX, you can:
- Overwrite selection columns
- Insert `JOIN` and `WHERE` conditions
- Transform queries into various SQL statements (`CREATE TABLE AS`, `INSERT INTO`, `UPDATE`, `DELETE`)

## 🚀 Advanced CTE Handling  
One of CarbunqleX's most powerful features is its **flexible handling of CTEs**.  
Normally, CTEs are limited to the `WITH` clause and cannot be directly referenced in `WHERE` or `JOIN` conditions.  
However, CarbunqleX detects existing CTEs and **lifts them to the top level**, allowing seamless inclusion in conditions where they would otherwise be invalid.  
This enables highly dynamic and reusable query transformations.

## 💥 Easy Insertion of Search Conditions  
Another standout feature of CarbunqleX is its **ease of inserting search conditions**.  
With AST parsing, the library automatically identifies the most effective point for insertion, such as within subqueries or CTEs, ensuring that the conditions are applied optimally.  

Unlike general libraries where you have to manually specify where to insert search conditions (usually at the top level of the query), CarbunqleX **automatically detects** the deepest relevant level of the query and inserts conditions at that level.  
For instance, when dealing with a `GROUP BY` query, it's crucial to insert search conditions **before** the grouping to ensure they are effective. CarbunqleX handles this seamlessly without you needing to worry about the hierarchical structure of the query!

## 🔥 Lightweight & Easy to Use  
- **Minimal dependencies** – Works directly with RawSQL  
- **No special setup or DBMS required** – Operates purely on query strings  
- **Seamless integration** – Works alongside general ORM libraries  

---

**Let's make RawSQL more powerful and reusable with CarbunqleX!**
