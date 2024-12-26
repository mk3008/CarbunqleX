using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.DatasourceExpressionTests;

public class TableSourceTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_WithNamespacesAndAlias_ReturnsCorrectSql()
    {
        // Arrange
        var namespaces = new List<string> { "dbo", "schema" };
        var tableName = "Users";
        var alias = "U";
        var tableSource = new TableSource(namespaces, tableName, alias);

        // Act
        var sql = tableSource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("dbo.schema.Users as U", sql);
    }

    [Fact]
    public void ToSql_WithNamespacesAndNoAlias_ReturnsCorrectSql()
    {
        // Arrange
        var namespaces = new List<string> { "dbo", "schema" };
        var tableName = "Users";
        var tableSource = new TableSource(namespaces, tableName, tableName);

        // Act
        var sql = tableSource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("dbo.schema.Users", sql);
    }

    [Fact]
    public void ToSql_WithNoNamespacesAndAlias_ReturnsCorrectSql()
    {
        // Arrange
        var tableName = "Users";
        var alias = "U";
        var tableSource = new TableSource(tableName, alias);

        // Act
        var sql = tableSource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("Users as U", sql);
    }

    [Fact]
    public void ToSql_WithNoNamespacesAndNoAlias_ReturnsCorrectSql()
    {
        // Arrange
        var tableName = "Users";
        var tableSource = new TableSource(tableName);

        // Act
        var sql = tableSource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("Users", sql);
    }


    [Fact]
    public void GetSelectableColumns_ShouldReturnCorrectColumns()
    {
        // Arrange
        var tableName = "Users";
        var alias = "U";
        var tableSource = new TableSource(tableName, alias, new List<string> { "Column1", "Column2", "Column3" });

        // Act
        var selectableColumns = tableSource.GetSelectableColumns();
        foreach (var column in selectableColumns)
        {
            output.WriteLine($"{column}");
        }

        // Assert
        var expectedColumns = new List<ColumnExpression>
        {
            new ColumnExpression("U", "Column1"),
            new ColumnExpression("U", "Column2"),
            new ColumnExpression("U", "Column3")
        };
        Assert.Equal(expectedColumns.Select(c => c.ColumnName), selectableColumns.Select(c => c));
    }

    [Fact]
    public void GetSelectableColumns_ShouldReturnEmpty_WhenNoColumnNames()
    {
        // Arrange
        var tableName = "Users";
        var tableSource = new TableSource(tableName);

        // Act
        var selectableColumns = tableSource.GetSelectableColumns();
        foreach (var column in selectableColumns)
        {
            output.WriteLine($"{column}");
        }

        // Assert
        Assert.Empty(selectableColumns);
    }
}
