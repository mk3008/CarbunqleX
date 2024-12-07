using Carbunqlex.DatasourceExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.DatasourceTests;

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
        var sql = tableSource.ToSql();
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
        var sql = tableSource.ToSql();
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
        var sql = tableSource.ToSql();
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
        var sql = tableSource.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("Users", sql);
    }
}
