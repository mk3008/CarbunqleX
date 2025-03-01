using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class GroupByClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_NoColumns_ReturnsEmptyString()
    {
        // Arrange
        var groupByClause = new GroupByClause();

        // Act
        var result = groupByClause.ToSqlWithoutCte();
        output.WriteLine(result);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToSql_WithColumns_ReturnsCorrectSql()
    {
        // Arrange
        var groupByClause = new GroupByClause();
        groupByClause.GroupByColumns.Add(new ColumnExpression("a", "value"));
        groupByClause.GroupByColumns.Add(new ColumnExpression("a", "id"));

        // Act
        var result = groupByClause.ToSqlWithoutCte();
        output.WriteLine(result);

        // Assert
        Assert.Equal("group by a.value, a.id", result);
    }
}
