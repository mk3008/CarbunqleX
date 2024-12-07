using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit;

namespace Carbunqlex.Tests;

public class PagingClauseTests
{
    [Fact]
    public void ToSql_ReturnsCorrectSql()
    {
        // Arrange
        var offset = new ConstantExpression(10);
        var fetch = new ConstantExpression(20);
        var pagingClause = new PagingClause(offset, fetch);

        // Act
        var sql = pagingClause.ToSql();

        // Assert
        Assert.Equal("offset 10 rows fetch next 20 rows only", sql);
    }

    [Fact]
    public void ToSql_WithParameterExpressions_ReturnsCorrectSql()
    {
        // Arrange
        var offset = new ParameterExpression("@offset", 10);
        var fetch = new ParameterExpression("@fetch", 20);
        var pagingClause = new PagingClause(offset, fetch);

        // Act
        var sql = pagingClause.ToSql();

        // Assert
        Assert.Equal("offset @offset rows fetch next @fetch rows only", sql);
    }
}
