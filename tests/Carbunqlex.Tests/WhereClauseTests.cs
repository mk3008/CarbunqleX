using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests;

public class WhereClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_WithCondition_ReturnsCorrectSql()
    {
        // Arrange
        var condition = new ColumnExpression("a", "value = 1");
        var whereClause = new WhereClause(condition);

        // Act
        var result = whereClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("where a.value = 1", result);
    }
}
