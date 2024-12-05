using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Carbunqlex.Tests;

public class WindowClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

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

        var windowClause = new WindowClause("w", windowFunction);

        // Act
        var result = windowClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("window w as (partition by a.value order by a.id rows between unbounded preceding and current row)", result);
    }
}
