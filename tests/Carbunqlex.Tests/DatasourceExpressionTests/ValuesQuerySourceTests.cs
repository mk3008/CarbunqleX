using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.QuerySources;
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
            ValueBuilder.Constant(1),
            ValueBuilder.Constant("test"),
            ValueBuilder.Null
        };
        var columns2 = new List<IValueExpression>
        {
            ValueBuilder.Constant(2),
            ValueBuilder.Constant("example"),
            ValueBuilder.Constant(new DateTime(2001,2,3))
        };
        var columns3 = new List<IValueExpression>
        {
            ValueBuilder.Constant(3),
            ValueBuilder.Constant("'O''Reilly'"),
            ValueBuilder.Null
        };
        query.AddRow(columns1);
        query.AddRow(columns2);
        query.AddRow(columns3);

        var alias = "testAlias";
        var columnAliases = new List<string> { "col1", "col2", "col3" };
        var valuesQuerySource = new DatasourceExpression(new SubQuerySource(query), alias, new ColumnAliasClause(columnAliases));

        // Act
        var sql = valuesQuerySource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        var expectedSql = $"({query.ToSql()}) as {alias}({string.Join(", ", columnAliases)})";
        Assert.Equal(expectedSql, sql);
    }
}
