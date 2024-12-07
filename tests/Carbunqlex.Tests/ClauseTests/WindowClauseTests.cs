using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

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

        var windowExpression = new WindowExpression("w", windowFunction);
        var windowClause = new WindowClause(windowExpression);

        // Act
        var result = windowClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("window w as (partition by a.value order by a.id rows between unbounded preceding and current row)", result);
    }

    [Fact]
    public void ToSql_WithMultipleWindowExpressions_ReturnsCorrectSql()
    {
        // Arrange
        var partitionBy1 = new PartitionByClause();
        partitionBy1.PartitionByColumns.Add(new ColumnExpression("a", "value1"));

        var orderBy1 = new OrderByClause();
        orderBy1.OrderByColumns.Add(new OrderByColumn(new ColumnExpression("a", "id1")));

        var windowFrame1 = new WindowFrame(
            WindowFrameBoundary.UnboundedPreceding,
            WindowFrameBoundary.CurrentRow,
            FrameType.Rows);

        var windowFunction1 = new WindowFunction(partitionBy1, orderBy1, windowFrame1);
        var windowExpression1 = new WindowExpression("w1", windowFunction1);

        var partitionBy2 = new PartitionByClause();
        partitionBy2.PartitionByColumns.Add(new ColumnExpression("b", "value2"));

        var orderBy2 = new OrderByClause();
        orderBy2.OrderByColumns.Add(new OrderByColumn(new ColumnExpression("b", "id2")));

        var windowFrame2 = new WindowFrame(
            WindowFrameBoundary.CurrentRow,
            WindowFrameBoundary.UnboundedFollowing,
            FrameType.Rows);

        var windowFunction2 = new WindowFunction(partitionBy2, orderBy2, windowFrame2);
        var windowExpression2 = new WindowExpression("w2", windowFunction2);

        var windowClause = new WindowClause(windowExpression1, windowExpression2);

        // Act
        var result = windowClause.ToSql();
        output.WriteLine(result);

        // Assert
        Assert.Equal("window w1 as (partition by a.value1 order by a.id1 rows between unbounded preceding and current row), w2 as (partition by b.value2 order by b.id2 rows between current row and unbounded following)", result);
    }
}
