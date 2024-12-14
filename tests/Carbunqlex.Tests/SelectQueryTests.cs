using Carbunqlex.Clauses;
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
        var selectQuery = SelectQueryFactory.CreateSelectQueryWithAllComponents();

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
        var selectQuery = SelectQueryFactory.CreateSelectQueryWithWithClause("cte");

        // Act
        var sql = selectQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte as (SELECT * FROM table) select ColumnName1 from cte", sql);
    }

    [Fact]
    public void ToSqlWithoutCte_WithWithClause_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery = SelectQueryFactory.CreateSelectQueryWithWithClause("cte");

        // Act
        var sql = selectQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 from cte", sql);
    }

    [Fact]
    public void ToSql_WithSubqueryWithClause_ReturnsCorrectSql()
    {
        // Arrange
        var subquery = SelectQueryFactory.CreateSelectQueryWithWithClause("cte_sub");
        output.WriteLine(subquery.ToSql());

        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("ColumnName1"))
        );
        var fromClause = new FromClause(
            new SubQuerySource(subquery, "subquery")
        );
        var selectQuery = new SelectQuery(selectClause, fromClause);

        // Act
        var sql = selectQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte_sub as (SELECT * FROM table) select ColumnName1 from (select ColumnName1 from cte_sub) as subquery", sql);
    }
    [Fact]
    public void ToSql_WithInlineQueryWithClause_ReturnsCorrectSql()
    {
        // Arrange
        var inlineQuery = SelectQueryFactory.CreateSelectQueryWithWithClause("cte_inline");
        output.WriteLine(inlineQuery.ToSql());

        var selectClause = new SelectClause(
            new SelectExpression(new InlineQuery(inlineQuery), "value")
        );
        var selectQuery = new SelectQuery(selectClause);

        // Act
        var sql = selectQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte_inline as (SELECT * FROM table) select (select ColumnName1 from cte_inline) as value", sql);
    }

}
