using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class WindowFrameTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_RowsFrame_ReturnsCorrectSql()
    {
        // Arrange
        var startExpression = new WindowFrameBoundaryKeyword("unbounded preceding");
        var endExpression = new WindowFrameBoundaryKeyword("current row");
        var windowFrame = new BetweenWindowFrame("rows", new BetweenWindowFrameBoundary(startExpression, endExpression));

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
        var startExpression = new WindowFrameBoundaryKeyword("unbounded preceding");
        var endExpression = new WindowFrameBoundaryKeyword("current row");
        var windowFrame = new BetweenWindowFrame("range", new BetweenWindowFrameBoundary(startExpression, endExpression));

        // Act
        var result = windowFrame.ToSqlWithoutCte();
        output.WriteLine(result);

        // Assert
        Assert.Equal("range between unbounded preceding and current row", result);
    }
}
