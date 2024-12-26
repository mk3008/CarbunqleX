using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class FromClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private static IDatasource GetDatasource()
    {
        return new TableSource("table_a", "a", new List<string> { "Column1", "Column2", "Column3" });
    }

    private static JoinClause GetInnerJoinClause()
    {
        var source = new TableSource("table_b", "b", new List<string> { "Column4", "Column5" });

        var condition = new BinaryExpression(
            "and",
            new BinaryExpression(
                "=",
                new ColumnExpression("a", "table_a_id"),
                new ColumnExpression("b", "table_a_id")
            ),
            new BinaryExpression(
                "=",
                new ColumnExpression("a", "table_a_sub_id"),
                new ColumnExpression("b", "table_a_sub_id")
            )
        );
        return new JoinClause(source, JoinType.Inner, condition);
    }

    [Fact]
    public void ToSql_NoJoins_ReturnsCorrectSql()
    {
        // Arrange
        var datasource = GetDatasource();
        var fromClause = new FromClause(datasource);

        // Act
        var sql = fromClause.ToSqlWithoutCte();

        // Assert
        Assert.Equal("from table_a as a", sql);
    }

    [Fact]
    public void ToSql_WithJoins_ReturnsCorrectSql()
    {
        // Arrange
        var datasource = GetDatasource();
        var joinClause = GetInnerJoinClause();
        var fromClause = new FromClause(datasource);
        fromClause.JoinClauses.Add(joinClause);

        // Act
        var sql = fromClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("from table_a as a inner join table_b as b on a.table_a_id = b.table_a_id and a.table_a_sub_id = b.table_a_sub_id", sql);
    }

    [Fact]
    public void GetSelectableColumns_NoJoins_ReturnsCorrectColumns()
    {
        // Arrange
        var datasource = GetDatasource();
        var fromClause = new FromClause(datasource);

        // Act
        var selectableColumns = fromClause.GetSelectableColumnExpressions();
        foreach (var column in selectableColumns)
        {
            output.WriteLine(column.ToSqlWithoutCte());
        }

        // Assert
        var expectedColumns = new List<string>
        {
            "a.Column1",
            "a.Column2",
            "a.Column3"
        };
        Assert.Equal(expectedColumns, selectableColumns.Select(c => c.ToSqlWithoutCte()));
    }

    [Fact]
    public void GetSelectableColumns_WithJoins_ReturnsCorrectColumns()
    {
        // Arrange
        var datasource = GetDatasource();
        var joinClause = GetInnerJoinClause();
        var fromClause = new FromClause(datasource);
        fromClause.JoinClauses.Add(joinClause);

        // Act
        var selectableColumns = fromClause.GetSelectableColumnExpressions();
        foreach (var column in selectableColumns)
        {
            output.WriteLine(column.ToSqlWithoutCte());
        }

        // Assert
        var expectedColumns = new List<string>
        {
            "a.Column1",
            "a.Column2",
            "a.Column3",
            "b.Column4",
            "b.Column5"
        };
        Assert.Equal(expectedColumns, selectableColumns.Select(c => c.ToSqlWithoutCte()));
    }
}
