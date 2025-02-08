using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class PagingClauseTests
{
    private readonly ITestOutputHelper output;

    public PagingClauseTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void OffsetClause_ToSql_ReturnsCorrectSql()
    {
        // Arrange
        var offset = new LiteralExpression(10);
        var offsetClause = new OffsetClause(offset);

        // Act
        var sql = offsetClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("offset 10", sql);
    }

    [Fact]
    public void FetchClause_ToSql_ReturnsCorrectSql()
    {
        // Arrange
        var fetch = new LiteralExpression(20);
        var fetchClause = new FetchClause("next", fetch, false, string.Empty);

        // Act
        var sql = fetchClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("fetch next 20", sql);
    }

    [Fact]
    public void LimitClause_ToSql_ReturnsCorrectSql()
    {
        // Arrange
        var limit = new LiteralExpression(30);
        var limitClause = new LimitClause(limit);

        // Act
        var sql = limitClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("limit 30", sql);
    }

    [Fact]
    public void OffsetClause_WithParameterExpressions_ReturnsCorrectSql()
    {
        // Arrange
        var offset = new ParameterExpression("@offset");
        var offsetClause = new OffsetClause(offset);

        // Act
        var sql = offsetClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("offset @offset", sql);
    }

    [Fact]
    public void FetchClause_WithParameterExpressions_ReturnsCorrectSql()
    {
        // Arrange
        var fetch = new ParameterExpression("@fetch");
        var fetchClause = new FetchClause("next", fetch, false, "rows only");

        // Act
        var sql = fetchClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("fetch next @fetch rows only", sql);
    }

    [Fact]
    public void LimitClause_WithParameterExpressions_ReturnsCorrectSql()
    {
        // Arrange
        var limit = new ParameterExpression("@limit");
        var limitClause = new LimitClause(limit);

        // Act
        var sql = limitClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("limit @limit", sql);
    }
}
