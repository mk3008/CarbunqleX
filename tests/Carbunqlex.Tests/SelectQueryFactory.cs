using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Tests;

public static class SelectQueryFactory
{
    public static SelectQuery CreateSelectQueryWithAllComponents()
    {
        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("ColumnName1")),
            new SelectExpression(new ColumnExpression("ColumnName2"), "alias2")
        );

        var fromClause = new FromClause(new TableSource("TableName"));

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

        var windowFunction = new WindowFunction(
            new PartitionByClause(new ColumnExpression("ColumnName1")),
            new OrderByClause(new OrderByColumn(new ColumnExpression("ColumnName2"))),
            new WindowFrame(WindowFrameBoundary.UnboundedPreceding, WindowFrameBoundary.CurrentRow, FrameType.Rows)
        );

        var windowClause = new WindowClause(new WindowExpression("w", windowFunction));

        var forClause = new ForClause(LockType.Update);

        var pagingClause = new PagingClause(new ConstantExpression(10), new ConstantExpression(20));

        var selectQuery = new SelectQuery(selectClause)
        {
            FromClause = fromClause,
            WhereClause = whereClause,
            ForClause = forClause,
            PagingClause = pagingClause
        };

        selectQuery.GroupByClause.GroupByColumns.AddRange(groupByClause.GroupByColumns);
        selectQuery.HavingClause.Conditions.AddRange(havingClause.Conditions);
        selectQuery.OrderByClause.OrderByColumns.AddRange(orderByClause.OrderByColumns);
        selectQuery.WindowClause.WindowExpressions.AddRange(windowClause.WindowExpressions);

        return selectQuery;
    }

    public static SelectQuery CreateSelectQueryWithWithClause(string cteName)
    {
        var commonTableClause = new CommonTableClause(new MockQuery("SELECT * FROM table"), cteName);

        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("ColumnName1"))
        );

        var fromClause = new FromClause(new TableSource(cteName));

        var selectQuery = new SelectQuery(selectClause, fromClause);
        selectQuery.WithClause.CommonTableClauses.Add(commonTableClause);

        return selectQuery;
    }

    public static SelectQuery CreateSelectQuery(string columnName)
    {
        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression(columnName))
        );

        return new SelectQuery(selectClause);
    }

    public static SelectQuery CreateSelectQueryWithParameters(Dictionary<string, object?> parameters)
    {
        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("ColumnName1"))
        );
        var selectQuery = new SelectQuery(selectClause);
        foreach (var parameter in parameters)
        {
            selectQuery.Parameters[parameter.Key] = parameter.Value;
        }
        return selectQuery;
    }

    // Simple implementation of IQuery for testing purposes
    private class MockQuery : IQuery
    {
        private readonly string _sql;

        public MockQuery(string sql)
        {
            _sql = sql;
        }

        public string ToSql()
        {
            return _sql;
        }

        public IEnumerable<Lexeme> GenerateLexemes()
        {
            return Enumerable.Empty<Lexeme>();
        }

        public string ToSqlWithoutCte()
        {
            return _sql;
        }

        public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
        {
            return Enumerable.Empty<Lexeme>();
        }

        public IEnumerable<CommonTableClause> GetCommonTableClauses()
        {
            return Enumerable.Empty<CommonTableClause>();
        }

        public IEnumerable<IQuery> GetQueries()
        {
            return Enumerable.Empty<IQuery>();
        }

        public IDictionary<string, object?> GetParameters()
        {
            return new Dictionary<string, object?>();
        }

        public IEnumerable<string> GetSelectedColumns()
        {
            throw new NotImplementedException();
        }
    }
}
