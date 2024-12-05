using Carbunqlex.Clauses;
using Xunit;
using System.Collections.Generic;
using Xunit.Abstractions;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Tests;

public class OrderByClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_NoColumns_ReturnsEmptyString()
    {
        // Arrange
        var orderByClause = new OrderByClause();

        // Act
        var result = orderByClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToSql_WithColumns_ReturnsCorrectSql()
    {
        // Arrange
        var orderByClause = new OrderByClause();
        orderByClause.OrderByColumns.Add(new OrderByColumn(new ColumnExpression("a", "value")));
        orderByClause.OrderByColumns.Add(new OrderByColumn(new ColumnExpression("a", "id"), false));

        // Act
        var result = orderByClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("order by a.value, a.id desc", result);
    }
}
