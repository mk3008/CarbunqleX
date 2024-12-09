﻿using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests;

public class SelectQueryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private ColumnExpression CreateColumnExpression(string columnName)
    {
        return new ColumnExpression(columnName);
    }

    [Fact]
    public void ToSql_WithAllComponents_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName1")),
            new SelectExpression(CreateColumnExpression("ColumnName2"), "alias2")
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
            CreateColumnExpression("ColumnName1"),
            CreateColumnExpression("ColumnName2")
        );

        var havingClause = new HavingClause(
            new BinaryExpression(
                ">",
                new ColumnExpression("ColumnName1"),
                new ConstantExpression(10)
            )
        );

        var orderByClause = new OrderByClause(
            new OrderByColumn(CreateColumnExpression("ColumnName1")),
            new OrderByColumn(CreateColumnExpression("ColumnName2"), ascending: false)
        );

        var windowFunction = new WindowFunction(
            new PartitionByClause(CreateColumnExpression("ColumnName1")),
            new OrderByClause(new OrderByColumn(CreateColumnExpression("ColumnName2"))),
            new WindowFrame(WindowFrameBoundary.UnboundedPreceding, WindowFrameBoundary.CurrentRow, FrameType.Rows)
        );

        var windowClause = new WindowClause(new WindowExpression("w", windowFunction));

        var forClause = new ForClause(LockType.Update);

        var pagingClause = new PagingClause(new ConstantExpression(10), new ConstantExpression(20));

        var selectQuery = new SelectQuery(selectClause)
        {
            FromClause = fromClause,
            WhereClause = whereClause,
            GroupByClause = groupByClause,
            HavingClause = havingClause,
            OrderByClause = orderByClause,
            WindowClause = windowClause,
            ForClause = forClause,
            PagingClause = pagingClause
        };

        // Act
        var sql = selectQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1, ColumnName2 as alias2 from TableName where ColumnName1 = 1 group by ColumnName1, ColumnName2 having ColumnName1 > 10 order by ColumnName1, ColumnName2 desc window w as (partition by ColumnName1 order by ColumnName2 rows between unbounded preceding and current row) for update offset 10 rows fetch next 20 rows only", sql);
    }

    [Fact]
    public void ToSql_WithMinimalComponents_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName1"))
        );

        var selectQuery = new SelectQuery(selectClause);

        // Act
        var sql = selectQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1", sql);
    }

    [Fact]
    public void ToSql_WithWithClause_ReturnsCorrectSql()
    {
        // Arrange
        var commonTableClause = new CommonTableClause(new SimpleQuery("SELECT * FROM table"), "cte");

        var selectClause = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName1"))
        );

        var fromClause = new FromClause(new TableSource("cte"));

        var selectQuery = new SelectQuery(selectClause, fromClause);
        selectQuery.WithClause.CommonTableClauses.Add(commonTableClause);

        // Act
        var sql = selectQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte as (SELECT * FROM table) select ColumnName1 from cte", sql);
    }

    // Simple implementation of IQuery for testing purposes
    private class SimpleQuery : IQuery
    {
        private readonly string _sql;

        public SimpleQuery(string sql)
        {
            _sql = sql;
        }

        public string ToSql()
        {
            return _sql;
        }

        public IEnumerable<Lexeme> GetLexemes()
        {
            return Enumerable.Empty<Lexeme>();
        }
    }
}
