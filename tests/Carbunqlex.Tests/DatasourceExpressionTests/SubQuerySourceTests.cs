using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.DatasourceExpressionTests;

public class SubQuerySourceTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSqlWithoutCte_WithValidAlias_ShouldReturnCorrectSql()
    {
        // Arrange
        var query = new SelectQuery(
            new SelectClause(
                new SelectExpression(
                    new LiteralExpression(1)
                    )
                )
            );
        var alias = "alias";
        var subQuerySource = new DatasourceExpression(new SubQuerySource(query), alias);

        // Act
        var sql = subQuerySource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("(select 1) as alias", sql);
    }

    [Fact]
    public void GetSelectableColumns_ShouldReturnCorrectColumns()
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
        var subQuerySource = new DatasourceExpression(new SubQuerySource(selectQuery), "subquery");

        // Act
        var selectableColumns = subQuerySource.GetSelectableColumns();
        foreach (var column in selectableColumns)
        {
            output.WriteLine($"{column}");
        }

        // Assert
        var expectedColumns = new List<ColumnExpression>
        {
            new ColumnExpression("subquery", "Alias1"),
            new ColumnExpression("subquery", "Alias2"),
            new ColumnExpression("subquery", "Alias3")
        };
        Assert.Equal(expectedColumns.Select(c => c.ColumnName), selectableColumns.Select(c => c));
    }
}
