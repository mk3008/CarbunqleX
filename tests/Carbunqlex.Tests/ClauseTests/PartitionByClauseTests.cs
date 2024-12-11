using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class PartitionByClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_NoColumns_ReturnsEmptyString()
    {
        // Arrange
        var partitionByClause = new PartitionByClause();

        // Act
        var result = partitionByClause.ToSqlWithoutCte();
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
        var result = partition.ToSqlWithoutCte();
        output.WriteLine(result);

        // Assert
        Assert.Equal("partition by a.value, a.id", result);
    }
}
