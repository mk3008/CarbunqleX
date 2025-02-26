using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.QuerySources;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryTests;

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
        Assert.Equal("select ColumnName1, ColumnName2 as alias2 from TableName where ColumnName1 = 1 group by ColumnName1, ColumnName2 having ColumnName1 > 10 order by ColumnName1, ColumnName2 desc window w as (partition by ColumnName1 order by ColumnName2 rows between unbounded preceding and current row) offset 10 fetch next 20 for update", sql);
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
        var selectQuery = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table", "cte");

        // Act
        var sql = selectQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte as (select * from table) select * from cte", sql);
    }

    [Fact]
    public void ToSqlWithoutCte_WithWithClause_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table", "cte");

        // Act
        var sql = selectQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select * from cte", sql);
    }

    [Fact]
    public void ToSql_WithSubqueryWithClause_ReturnsCorrectSql()
    {
        // Arrange
        var subquery = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table", "cte_sub");
        output.WriteLine(subquery.ToSql());

        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("ColumnName1"))
        );
        var fromClause = new FromClause(
            new DatasourceExpression(new SubQuerySource(subquery), "subquery")
        );
        var selectQuery = new SelectQuery(selectClause, fromClause);

        // Act
        var sql = selectQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte_sub as (select * from table) select ColumnName1 from (select * from cte_sub) as subquery", sql);
    }

    [Fact]
    public void ToSql_WithInlineQueryWithClause_ReturnsCorrectSql()
    {
        // Arrange
        var inlineQuery = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table", "cte_inline");
        output.WriteLine(inlineQuery.ToSql());

        var selectClause = new SelectClause(
            new SelectExpression(new InlineQuery(inlineQuery), "value")
        );
        var selectQuery = new SelectQuery(selectClause);

        // Act
        var sql = selectQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte_inline as (select * from table) select (select * from cte_inline) as value", sql);
    }

    [Fact]
    public void ToSql_WithNestedWithClauses_ReturnsCorrectSql()
    {
        // Arrange
        var selectQueryWithCte = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table", "inner_cte");
        output.WriteLine(selectQueryWithCte.ToSql());

        var commonTableIncludeCte = new CommonTableClause(selectQueryWithCte, "outer_cte");
        var withClause = new WithClause(commonTableIncludeCte);

        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("ColumnName1"))
        );
        var fromClause = new FromClause(new DatasourceExpression(new TableSource("outer_cte")));
        var selectQuery = new SelectQuery(selectClause, fromClause);
        selectQuery.WithClause.CommonTableClauses.AddRange(withClause.CommonTableClauses);

        // Act
        var sql = selectQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with inner_cte as (select * from table), outer_cte as (select * from inner_cte) select ColumnName1 from outer_cte", sql);
    }

    [Fact]
    public void GetParameters_ShouldReturnOwnParameters()
    {
        // Arrange
        var selectClause = new SelectClause();
        var selectQuery = new SelectQuery(selectClause);
        selectQuery.Parameters["param1"] = "value1";
        selectQuery.Parameters["param2"] = 123;

        // Act
        var parameters = selectQuery.GetParameters();
        foreach (var parameter in parameters)
        {
            output.WriteLine($"{parameter.Key}: {parameter.Value}");
        }

        // Assert
        Assert.Equal(2, parameters.Count);
        Assert.Equal("value1", parameters["param1"]);
        Assert.Equal(123, parameters["param2"]);
    }

    [Fact]
    public void GetParameters_ShouldIncludeInternalParameters()
    {
        // Arrange
        var selectClause = new SelectClause();
        var internalQuery = SelectQueryFactory.CreateSelectAllQueryWithParameters("table", new Dictionary<string, object?>
        {
            { "internalParam", "internalValue" }
        });
        var fromClause = new FromClause(new DatasourceExpression(new SubQuerySource(internalQuery), "t"));
        var selectQuery = new SelectQuery(selectClause, fromClause);
        selectQuery.Parameters["param1"] = "value1";

        // Act
        var parameters = selectQuery.GetParameters();
        foreach (var parameter in parameters)
        {
            output.WriteLine($"{parameter.Key}: {parameter.Value}");
        }

        // Assert
        Assert.Equal(2, parameters.Count);
        Assert.Equal("value1", parameters["param1"]);
        Assert.Equal("internalValue", parameters["internalParam"]);
    }

    [Fact]
    public void GetParameters_ShouldPrioritizeOwnParameters()
    {
        // Arrange
        var selectClause = new SelectClause();
        var internalQuery = SelectQueryFactory.CreateSelectAllQueryWithParameters("table", new Dictionary<string, object?>
        {
            { "param1", "internalValue" }
        });
        var fromClause = new FromClause(new DatasourceExpression(new SubQuerySource(internalQuery), "t"));
        var selectQuery = new SelectQuery(selectClause, fromClause);
        selectQuery.Parameters["param1"] = "ownValue";

        // Act
        var parameters = selectQuery.GetParameters();
        foreach (var parameter in parameters)
        {
            output.WriteLine($"{parameter.Key}: {parameter.Value}");
        }

        // Assert
        Assert.Single(parameters);
        Assert.Equal("ownValue", parameters["param1"]);
    }

    [Fact]
    public void GetSelectedColumns_ReturnsCorrectColumns()
    {
        // Arrange
        var selectExpressions = new List<SelectExpression>
        {
            new SelectExpression(new ColumnExpression("Column1"), "Alias1"),
            new SelectExpression(new ColumnExpression("Column2"), "Alias2"),
            new SelectExpression(new ColumnExpression("Column3"), "Alias3")
        };
        var selectClause = new SelectClause(selectExpressions.ToArray());
        var selectQuery = new SelectQuery(selectClause);

        // Act
        var selectedColumns = selectQuery.GetSelectExpressions();
        foreach (var column in selectedColumns)
        {
            output.WriteLine(column.Alias);
        }

        // Assert
        var expectedColumns = new List<string> { "Alias1", "Alias2", "Alias3" };
        Assert.Equal(expectedColumns, selectedColumns.Select(x => x.Alias).ToList());
    }
}
