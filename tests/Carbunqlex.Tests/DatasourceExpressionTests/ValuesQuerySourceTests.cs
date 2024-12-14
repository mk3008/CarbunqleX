using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.DatasourceExpressionTests;

public class ValuesQuerySourceTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_ShouldReturnCorrectSqlString()
    {
        // Arrange
        var query = new ValuesQuery();
        var columns1 = new List<IValueExpression>
        {
            ConstantExpression.Create(1),
            ConstantExpression.Create("test"),
            ConstantExpression.Create(null)
        };
        var columns2 = new List<IValueExpression>
        {
            ConstantExpression.Create(2),
            ConstantExpression.Create("example"),
            ConstantExpression.Create(new DateTime(2001,2,3))
        };
        var columns3 = new List<IValueExpression>
        {
            ConstantExpression.Create(3),
            ConstantExpression.Create("O'Reilly"),
            ConstantExpression.Create(null)
        };
        query.AddRow(columns1);
        query.AddRow(columns2);
        query.AddRow(columns3);

        var alias = "testAlias";
        var columnAliases = new List<string> { "col1", "col2", "col3" };
        var valuesQuerySource = new SubQuerySource(query, alias, columnAliases);

        // Act
        var sql = valuesQuerySource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        var expectedSql = $"({query.ToSql()}) as {alias}({string.Join(", ", columnAliases)})";
        Assert.Equal(expectedSql, sql);
    }
}
