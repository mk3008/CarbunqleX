using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Tests;

public static class SelectQueryFactory
{
    public static SelectQuery CreateSelectQueryForTodayTotalPrice()
    {
        /*
            SELECT 
                s.sale_id
                , s.sale_date
                , s.price * s.amount AS total_price
                , p.name AS product_name
            FROM 
                sale AS s
                INNER JOIN product AS p ON s.product_id = p.product_id
            WHERE 
                s.sale_date = @today_date
         */

        // Create the select expressions
        var selectExpressions = new SelectClause(
            new SelectExpression(new ColumnExpression("s", "sale_id")),
            new SelectExpression(new ColumnExpression("s", "sale_date")),
            new SelectExpression(new BinaryExpression(
                "*",
                new ColumnExpression("s", "price"),
                new ColumnExpression("s", "amount")
            ), "total_price"),
            new SelectExpression(new ColumnExpression("p", "name"), "product_name")
        );

        // Create the from clause
        var fromClause = new FromClause(new DatasourceExpression(new TableSource("sale"), "sale"));

        // Create the join clause for the products table
        var joinClause = new JoinClause(
            new DatasourceExpression(new TableSource("product"), "product"),
            "inner join",
            new BinaryExpression(
                "=",
                new ColumnExpression("s", "product_id"),
                new ColumnExpression("p", "product_id")
            )
        );

        // Attach the join clause to the from clause
        fromClause.JoinClauses.Add(joinClause);

        // Create the where clause for today's date
        var whereClause = new WhereClause(
            new BinaryExpression(
                "=",
                new ColumnExpression("s", "sale_date"),
                new ParameterExpression("today_date") // Parameterized query
            )
        );

        // Create the select query
        var selectQuery = new SelectQuery(selectExpressions)
        {
            FromClause = fromClause,
        };
        selectQuery.WhereClause.Add(whereClause.Condition!);

        return selectQuery;
    }

    public static SelectQuery CreateSelectQueryForDailySummary()
    {
        /*
           SELECT 
                s.sale_date,
                SUM(s.price * s.amount) AS daily_total
            FROM 
                sale AS s
            WHERE 
                s.sale_date >= CURRENT_DATE - INTERVAL '7 days'
            GROUP BY 
                s.sale_date
         */
        var selectExpressions = new SelectClause(
            new SelectExpression(new ColumnExpression("s", "sale_date")),
            new SelectExpression(
                ValueBuilder.Function(
                    "SUM",
                    new BinaryExpression(
                        "*",
                        new ColumnExpression("s", "price"),
                        new ColumnExpression("s", "amount")
                    )
                ),
                "daily_total"
            )
        );

        var fromClause = new FromClause(new DatasourceExpression(new TableSource("sale"), "sale"));

        var whereClause = new WhereClause(
            new BinaryExpression(
                ">=",
                new ColumnExpression("s", "sale_date"),
                new BinaryExpression(
                    "-",
                    new ConstantExpression("CURRENT_DATE"),
                    new ModifierExpression("INTERVAL", new ConstantExpression("'7 days'"))
                )
            )
        );

        var selectQuery = new SelectQuery(selectExpressions)
        {
            FromClause = fromClause,
        };
        selectQuery.WhereClause.Add(whereClause.Condition!);
        selectQuery.GroupByClause.GroupByColumns.AddRange(
            [
                new ColumnExpression("s", "sale_date")
            ]);

        return selectQuery;
    }

    public static SelectQuery CreateSelectQueryForCategorySummary()
    {
        var selectClause = new SelectClause(
          new SelectExpression(new ColumnExpression("c", "category_name")),
          new SelectExpression(new ColumnExpression("s", "sale_date")),
          new SelectExpression(ValueBuilder.Function(
              "SUM",
              new BinaryExpression(
                  "*",
                  new ColumnExpression("s", "price"),
                  new ColumnExpression("s", "amount")
              )
          ), "category_total")
      );

        var fromClause = new FromClause(new DatasourceExpression(new TableSource("sale"), "sale"));

        var joinProduct = new JoinClause(
            new DatasourceExpression(new TableSource("product"), "product"),
            "inner join",
            new BinaryExpression(
                "=",
                new ColumnExpression("s", "product_id"),
                new ColumnExpression("p", "product_id")
            )
        );

        var joinCategory = new JoinClause(
            new DatasourceExpression(new TableSource("category"), "category"),
            "inner join",
            new BinaryExpression(
                "=",
                new ColumnExpression("p", "category_id"),
                new ColumnExpression("c", "category_id")
            )
        );

        fromClause.JoinClauses.Add(joinProduct);
        fromClause.JoinClauses.Add(joinCategory);

        var whereClause = new WhereClause(
            new BinaryExpression(
                ">=",
                new ColumnExpression("s", "sale_date"),
                new BinaryExpression(
                    "-",
                    new ConstantExpression("CURRENT_DATE"),
                    new ModifierExpression("INTERVAL", new ConstantExpression("7 days"))
                )
            )
        );

        var selectQuery = new SelectQuery(selectClause)
        {
            FromClause = fromClause,
        };
        selectQuery.WhereClause.Add(whereClause.Condition!);
        selectQuery.GroupByClause.GroupByColumns.AddRange(
            [
                new ColumnExpression("c", "category_name"),
                new ColumnExpression("s", "sale_date")
            ]);

        return selectQuery;
    }

    public static SelectQuery CreateComplexSelectQuery()
    {
        /*
        WITH
        DailySales AS (
            SELECT 
                s.sale_date,
                SUM(s.price * s.amount) AS daily_total
            FROM 
                sale AS s
            WHERE 
                s.sale_date >= CURRENT_DATE - INTERVAL '7 days' -- 過去7日間の売上
            GROUP BY 
                s.sale_date
        ),
        CategorySales AS (
            SELECT 
                c.category_name,
                s.sale_date,
                SUM(s.price * s.amount) AS category_total
            FROM 
                sale AS s
                INNER JOIN product AS p ON s.product_id = p.product_id
                INNER JOIN category AS c ON p.category_id = c.category_id
            WHERE 
                s.sale_date >= CURRENT_DATE - INTERVAL '7 days' -- 過去7日間の売上
            GROUP BY 
                c.category_name, s.sale_date
        )
        SELECT 
            s.sale_id,
            s.sale_date,
            s.price * s.amount AS total_price,
            p.name AS product_name,
            c.category_name,
            ds.daily_total,
            cs.category_total,
            (s.price * s.amount) / ds.daily_total AS daily_ratio,
            (s.price * s.amount) / cs.category_total AS category_ratio
        FROM 
            sale AS s
            INNER JOIN product AS p ON s.product_id = p.product_id
            INNER JOIN category AS c ON p.category_id = c.category_id
            INNER JOIN DailySales AS ds ON s.sale_date = ds.sale_date
            INNER JOIN CategorySales AS cs ON s.sale_date = cs.sale_date AND c.category_name = cs.category_name
        WHERE 
            s.sale_date >= CURRENT_DATE - INTERVAL '7 days'
        ORDER BY 
            s.sale_date, cs.category_total DESC;
         */

        // Create the CTE for DailySales
        var dailySalesCte = new CommonTableClause(CreateSelectQueryForDailySummary(), "DailySales");

        // Create the CTE for CategorySales
        var categorySalesCte = new CommonTableClause(CreateSelectQueryForCategorySummary(), "CategorySales");

        // Create the main query
        var mainSelectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("s", "sale_id")),
            new SelectExpression(new ColumnExpression("s", "sale_date")),
            new SelectExpression(new BinaryExpression(
                "*",
                new ColumnExpression("s", "price"),
                new ColumnExpression("s", "amount")
            ), "total_price"),
            new SelectExpression(new ColumnExpression("p", "name"), "product_name"),
            new SelectExpression(new ColumnExpression("c", "category_name")),
            new SelectExpression(new ColumnExpression("ds", "daily_total")),
            new SelectExpression(new ColumnExpression("cs", "category_total")),
            new SelectExpression(new BinaryExpression(
                "/",
                new BinaryExpression(
                    "*",
                    new ColumnExpression("s", "price"),
                    new ColumnExpression("s", "amount")
                ),
                new ColumnExpression("ds", "daily_total")
            ), "daily_ratio"),
            new SelectExpression(new BinaryExpression(
                "/",
                new BinaryExpression(
                    "*",
                    new ColumnExpression("s", "price"),
                    new ColumnExpression("s", "amount")
                ),
                new ColumnExpression("cs", "category_total")
            ), "category_ratio")
        );

        var mainFromClause = new FromClause(new DatasourceExpression(new TableSource("sale"), "sale"));

        var mainJoinProduct = new JoinClause(
            new DatasourceExpression(new TableSource("product"), "product"),
            "inner join",
            new BinaryExpression(
                "=",
                new ColumnExpression("s", "product_id"),
                new ColumnExpression("p", "product_id")
            )
        );

        var mainJoinCategory = new JoinClause(
            new DatasourceExpression(new TableSource("category"), "c"),
            "inner join",
            new BinaryExpression(
                "=",
                new ColumnExpression("p", "category_id"),
                new ColumnExpression("c", "category_id")
            )
        );

        var mainJoinDailySales = new JoinClause(
            new DatasourceExpression(new TableSource("DailySales"), "ds"),
            "inner join",
            new BinaryExpression(
                "=",
                new ColumnExpression("s", "sale_date"),
                new ColumnExpression("ds", "sale_date")
            )
        );

        var mainJoinCategorySales = new JoinClause(
            new DatasourceExpression(new TableSource("CategorySales"), "cs"),
            "inner join",
            new BinaryExpression(
                "AND",
                new BinaryExpression(
                    "=",
                    new ColumnExpression("s", "sale_date"),
                    new ColumnExpression("cs", "sale_date")
                ),
                new BinaryExpression(
                    "=",
                    new ColumnExpression("c", "category_name"),
                    new ColumnExpression("cs", "category_name")
                )
            )
        );

        mainFromClause.JoinClauses.Add(mainJoinProduct);
        mainFromClause.JoinClauses.Add(mainJoinCategory);
        mainFromClause.JoinClauses.Add(mainJoinDailySales);
        mainFromClause.JoinClauses.Add(mainJoinCategorySales);

        var mainWhereClause = new WhereClause(
            new BinaryExpression(
                ">=",
                new ColumnExpression("s", "sale_date"),
                new BinaryExpression(
                    "-",
                    new ConstantExpression("CURRENT_DATE"),
                    new ModifierExpression("INTERVAL", new ConstantExpression("7 days"))
                )
            )
        );

        var mainQuery = new SelectQuery(mainSelectClause)
        {
            FromClause = mainFromClause,
        };
        mainQuery.WhereClause.Add(mainWhereClause.Condition!);
        mainQuery.OrderByClause.OrderByColumns.AddRange(
            [
                new OrderByColumn(new ColumnExpression("s", "sale_date")),
                new OrderByColumn(new ColumnExpression("cs", "category_total"), ascending: false)
            ]);

        mainQuery.WithClause.CommonTableClauses.Add(dailySalesCte);
        mainQuery.WithClause.CommonTableClauses.Add(categorySalesCte);

        return mainQuery;
    }

    public static SelectQuery CreateSelectQueryWithAllComponents()
    {
        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("ColumnName1")),
            new SelectExpression(new ColumnExpression("ColumnName2"), "alias2")
        );

        var fromClause = new FromClause(new DatasourceExpression(new TableSource("TableName")));

        var whereClause = new WhereClause(
            new BinaryExpression(
                "=",
                new ColumnExpression("ColumnName1"),
                new ConstantExpression(1)
            )
        );

        var groupByClause = new GroupByClause(
            new ColumnExpression("ColumnName1"),
            new ColumnExpression("ColumnName2")
        );

        var havingClause = new HavingClause(
            new BinaryExpression(
                ">",
                new ColumnExpression("ColumnName1"),
                new ConstantExpression(10)
            )
        );

        var orderByClause = new OrderByClause(
            new OrderByColumn(new ColumnExpression("ColumnName1")),
            new OrderByColumn(new ColumnExpression("ColumnName2"), ascending: false)
        );

        var windowFunction = new NamelessWindowDefinition(
            new PartitionByClause(new ColumnExpression("ColumnName1")),
            new OrderByClause(new OrderByColumn(new ColumnExpression("ColumnName2"))),
            new BetweenWindowFrame(
                "rows",
                new BetweenWindowFrameBoundary(
                    new WindowFrameBoundaryKeyword("unbounded preceding"),
                    new WindowFrameBoundaryKeyword("current row")
                    )
                )
        );

        var windowClause = new WindowClause(new WindowExpression("w", windowFunction));

        var forClause = new ForClause("update");

        var offsetClause = new OffsetClause(new ConstantExpression(10));
        var fetchClause = new FetchClause("next", new ConstantExpression(20), false, string.Empty);

        var selectQuery = new SelectQuery(selectClause)
        {
            FromClause = fromClause,
            ForClause = forClause,
            OffsetClause = offsetClause,
            LimitClause = fetchClause
        };
        selectQuery.WhereClause.Add(whereClause.Condition!);
        selectQuery.GroupByClause.GroupByColumns.AddRange(groupByClause.GroupByColumns);
        selectQuery.HavingClause.Conditions.AddRange(havingClause.Conditions);
        selectQuery.OrderByClause.OrderByColumns.AddRange(orderByClause.OrderByColumns);
        selectQuery.WindowClause.WindowExpressions.AddRange(windowClause.WindowExpressions);

        return selectQuery;
    }

    public static UnionQuery CreateSelectRecursiveQuery()
    {
        // Define the recursive CTE query
        var recursiveQueryPrimary = new SelectQuery(
            new SelectClause(
                new SelectExpression(new ConstantExpression(1), "number")
            )
        );
        var recursiveQuerySecondary = new SelectQuery(
            new SelectClause(
                new SelectExpression(
                    new BinaryExpression(
                        "+",
                        new ColumnExpression("number"),
                        new ConstantExpression(1)
                    ),
                    "number"
                )
            ),
            new FromClause(new DatasourceExpression(new TableSource("number_series")))
        );
        var WhereClause = new WhereClause(
            new BinaryExpression(
                "<",
                new ColumnExpression("number"),
                new ConstantExpression(10)
                )
            );
        recursiveQuerySecondary.WhereClause.Add(WhereClause.Condition!);

        var unionAllQuery = new UnionQuery(
            UnionType.UnionAll,
            recursiveQueryPrimary,
            recursiveQuerySecondary
        );

        return unionAllQuery;
    }

    public static SelectQuery CreateSelectQueryWithSubQuery(string tableName, string tableAlias, string subQueryAlias, params string[] columns)
    {
        var subQuery = CreateSelectQuery(tableName, tableAlias, columns);

        var selectClause = new SelectClause(
            columns.Select(column => new SelectExpression(new ColumnExpression(subQueryAlias, column))).ToList()
        );

        var fromClause = new FromClause(new DatasourceExpression(new SubQuerySource(subQuery), subQueryAlias));

        var query = new SelectQuery(selectClause, fromClause);

        return query;
    }

    public static SelectQuery CreateSelectAllQueryWithWithClause(string tableName, string cteName)
    {
        var commonTableClause = new CommonTableClause(CreateSelectAllQuery(tableName), cteName);

        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("*"))
        );
        var fromClause = new FromClause(new DatasourceExpression(new TableSource(cteName)));

        var selectQuery = new SelectQuery(selectClause, fromClause);
        selectQuery.WithClause.CommonTableClauses.Add(commonTableClause);

        return selectQuery;
    }

    public static SelectQuery CreateSelectQuery(string tableName, string tableAlias, params string[] columns)
    {
        var selectClause = new SelectClause(
            columns.ToList().Select(column => new SelectExpression(new ColumnExpression(tableAlias, column))).ToList()
        );
        var fromClause = new FromClause(new DatasourceExpression(new TableSource(tableName), tableAlias));
        return new SelectQuery(selectClause, fromClause);
    }

    public static SelectQuery CreateSelectAllQuery(string tableName)
    {
        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("*"))
        );
        var fromClause = new FromClause(new DatasourceExpression(new TableSource(tableName)));
        return new SelectQuery(selectClause, fromClause);
    }

    public static SelectQuery CreateSelectConstantQuery(object value, string alias)
    {
        var selectClause = new SelectClause(
            new SelectExpression(ValueBuilder.Constant(value), alias)
        );
        return new SelectQuery(selectClause);
    }

    public static SelectQuery CreateSelectAllQueryWithParameters(string tableName, Dictionary<string, object?> parameters)
    {
        var selectQuery = CreateSelectAllQuery(tableName);
        foreach (var parameter in parameters)
        {
            selectQuery.Parameters[parameter.Key] = parameter.Value;
        }
        return selectQuery;
    }
}
