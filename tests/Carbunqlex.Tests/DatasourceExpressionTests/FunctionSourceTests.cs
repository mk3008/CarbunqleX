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
        var arguments = new List<IValueExpression> { new ConstantExpression(1), new ConstantExpression(2) };
        var alias = "TestAlias";
        var columnAliases = new ColumnAliases(new List<string> { "col1", "col2" });
        var functionSource = new FunctionSource(functionName, arguments, alias, columnAliases);

        // Act
        var sql = functionSource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("TestFunction(1, 2) as TestAlias(col1, col2)", sql);
    }
}
