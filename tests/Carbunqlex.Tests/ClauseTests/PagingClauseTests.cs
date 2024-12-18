﻿using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class PagingClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_ReturnsCorrectSql()
    {
        // Arrange
        var offset = new ConstantExpression(10);
        var fetch = new ConstantExpression(20);
        var pagingClause = new PagingClause(offset, fetch);

        // Act
        var sql = pagingClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("offset 10 rows fetch next 20 rows only", sql);
    }

    [Fact]
    public void ToSql_WithParameterExpressions_ReturnsCorrectSql()
    {
        // Arrange
        var offset = new ParameterExpression("@offset");
        var fetch = new ParameterExpression("@fetch");
        var pagingClause = new PagingClause(offset, fetch);

        // Act
        var sql = pagingClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("offset @offset rows fetch next @fetch rows only", sql);
    }
}
