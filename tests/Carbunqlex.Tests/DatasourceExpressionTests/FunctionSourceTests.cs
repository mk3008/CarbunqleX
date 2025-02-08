using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.DatasourceExpressionTests;

public class FunctionSourceTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_ShouldReturnCorrectSqlString()
    {
        // Arrange
        var functionName = "TestFunction";
        var arguments = new List<IValueExpression> { new LiteralExpression(1), new LiteralExpression(2) };
        var alias = "TestAlias";
        var columnAliases = new ColumnAliasClause(new List<string> { "col1", "col2" });
        var functionSource = new DatasourceExpression(new FunctionSource(functionName, arguments), alias, columnAliases);

        // Act
        var sql = functionSource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("TestFunction(1, 2) as TestAlias(col1, col2)", sql);
    }


    [Fact]
    public void GetSelectableColumns_ShouldReturnCorrectColumns()
    {
        // Arrange
        var functionName = "TestFunction";
        var arguments = new List<IValueExpression> { new LiteralExpression(1), new LiteralExpression(2) };
        var alias = "TestAlias";
        var columnAliases = new ColumnAliasClause(new List<string> { "col1", "col2" });
        var functionSource = new DatasourceExpression(new FunctionSource(functionName, arguments), alias, columnAliases);

        // Act
        var selectableColumns = functionSource.GetSelectableColumns();
        foreach (var column in selectableColumns)
        {
            output.WriteLine($"{column}");
        }

        // Assert
        var expectedColumns = new List<ColumnExpression>
        {
            new ColumnExpression("TestAlias", "col1"),
            new ColumnExpression("TestAlias", "col2")
        };
        Assert.Equal(expectedColumns.Select(c => c.ColumnName), selectableColumns.Select(c => c));
    }

    [Fact]
    public void GetSelectableColumns_ShouldReturnEmpty_WhenNoColumnAliases()
    {
        // Arrange
        var functionName = "TestFunction";
        var arguments = new List<IValueExpression> { new LiteralExpression(1), new LiteralExpression(2) };
        var alias = "TestAlias";
        var functionSource = new DatasourceExpression(new FunctionSource(functionName, arguments), alias);

        // Act
        var selectableColumns = functionSource.GetSelectableColumns();
        foreach (var column in selectableColumns)
        {
            output.WriteLine($"{column}");
        }

        // Assert
        Assert.Empty(selectableColumns);
    }
}
