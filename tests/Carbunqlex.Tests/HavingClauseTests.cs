using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests;

public class HavingClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_WithCondition_ReturnsCorrectSql()
    {
        // Arrange
        var condition = new ColumnExpression("a", "value = 1");
        var havingClause = new HavingClause(condition);

        // Act
        var result = havingClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("having a.value = 1", result);
    }
}
