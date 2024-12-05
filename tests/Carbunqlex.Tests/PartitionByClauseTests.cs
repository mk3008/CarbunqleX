using Carbunqlex.Clauses;
using Xunit;
using System.Collections.Generic;
using Xunit.Abstractions;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Tests;

public class PartitionByClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_NoColumns_ReturnsEmptyString()
    {
        // Arrange
        var partitionByClause = new PartitionByClause();

        // Act
        var result = partitionByClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToSql_WithColumns_ReturnsCorrectSql()
    {
        // Arrange
        var partition = new PartitionByClause();
        partition.PartitionByColumns.Add(new ColumnExpression("a", "value"));
        partition.PartitionByColumns.Add(new ColumnExpression("a", "id"));

        // Act
        var result = partition.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("partition by a.value, a.id", result);
    }
}
