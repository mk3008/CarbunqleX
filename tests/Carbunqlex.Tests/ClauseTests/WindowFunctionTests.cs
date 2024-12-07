using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class WindowFunctionTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_NoComponents_ReturnsEmptyString()
    {
        // Arrange
        var windowFunction = new WindowFunction();

        // Act
        var result = windowFunction.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToSql_WithAllComponents_ReturnsCorrectSql()
    {
        // Arrange
        var partitionBy = new PartitionByClause();
        partitionBy.PartitionByColumns.Add(new ColumnExpression("a", "value"));

        var orderBy = new OrderByClause();
        orderBy.OrderByColumns.Add(new OrderByColumn(new ColumnExpression("a", "id")));

        var windowFrame = new WindowFrame(
            WindowFrameBoundary.UnboundedPreceding,
            WindowFrameBoundary.CurrentRow,
            FrameType.Rows);

        var windowFunction = new WindowFunction(partitionBy, orderBy, windowFrame);

        // Act
        var result = windowFunction.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("partition by a.value order by a.id rows between unbounded preceding and current row", result);
    }
}
