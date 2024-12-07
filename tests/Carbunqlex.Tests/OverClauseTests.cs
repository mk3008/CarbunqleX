using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests;

public class OverClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_WithWindowFunction_ReturnsCorrectSql()
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

        var overClause = new OverClause(windowFunction);

        // Act
        var result = overClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("over (partition by a.value order by a.id rows between unbounded preceding and current row)", result);
    }
}
