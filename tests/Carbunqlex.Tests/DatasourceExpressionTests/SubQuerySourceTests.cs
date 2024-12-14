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
                    new ConstantExpression(1)
                    )
                )
            );
        var alias = "alias";
        var subQuerySource = new SubQuerySource(query, alias);

        // Act
        var sql = subQuerySource.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("(select 1) as alias", sql);
    }
}
