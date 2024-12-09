using Carbunqlex.Clauses;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class WindowFrameTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_RowsFrame_ReturnsCorrectSql()
    {
        // Arrange
        var startExpression = WindowFrameBoundary.UnboundedPreceding;
        var endExpression = WindowFrameBoundary.CurrentRow;
        var windowFrame = new WindowFrame(startExpression, endExpression, FrameType.Rows);

        // Act
        var result = windowFrame.ToSqlWithoutCte();
        output.WriteLine(result);

        // Assert
        Assert.Equal("rows between unbounded preceding and current row", result);
    }

    [Fact]
    public void ToSql_RangeFrame_ReturnsCorrectSql()
    {
        // Arrange
        var startExpression = WindowFrameBoundary.UnboundedPreceding;
        var endExpression = WindowFrameBoundary.CurrentRow;
        var windowFrame = new WindowFrame(startExpression, endExpression, FrameType.Range);

        // Act
        var result = windowFrame.ToSqlWithoutCte();
        output.WriteLine(result);

        // Assert
        Assert.Equal("range between unbounded preceding and current row", result);
    }
}
