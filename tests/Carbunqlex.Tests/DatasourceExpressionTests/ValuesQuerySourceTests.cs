using Carbunqlex.DatasourceExpressions;
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
        var columns1 = new List<ValuesColumn>
        {
            ValuesColumn.Create(1),
            ValuesColumn.Create("test"),
            ValuesColumn.Create(null)
        };
        var columns2 = new List<ValuesColumn>
        {
            ValuesColumn.Create(2),
            ValuesColumn.Create("example"),
            ValuesColumn.Create(new DateTime(2001,2,3))
        };
        var columns3 = new List<ValuesColumn>
        {
            ValuesColumn.Create(3),
            ValuesColumn.Create("O'Reilly"),
            ValuesColumn.Create(null)
        };
        query.AddRow(columns1);
        query.AddRow(columns2);
        query.AddRow(columns3);

        var alias = "testAlias";
        var columnAliases = new List<string> { "col1", "col2", "col3" };
        var valuesQuerySource = new ValuesQuerySource(query, alias, columnAliases);

        // Act
        var sql = valuesQuerySource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        var expectedSql = $"({query.ToSql()}) as {alias}({string.Join(", ", columnAliases)})";
        Assert.Equal(expectedSql, sql);
    }
}
