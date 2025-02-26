using Carbunqlex.Lexing;
using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class WindowFrameParserTests
{
    public WindowFrameParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_RowsFrame_ReturnsCorrectWindowFrame()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("rows between unbounded preceding and current row");

        // Act
        var result = WindowFrameParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("rows between unbounded preceding and current row", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_RangeFrame_ReturnsCorrectWindowFrame()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("range between 1 preceding and 1 following");

        // Act
        var result = WindowFrameParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("range between 1 preceding and 1 following", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_RowsFrameWithBoundaries_ReturnsCorrectWindowFrame()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("rows between 2 preceding and 2 following");

        // Act
        var result = WindowFrameParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("rows between 2 preceding and 2 following", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_RowsUnboundedPreceding_ReturnsCorrectWindowFrame()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("rows unbounded preceding");

        // Act
        var result = WindowFrameParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("rows unbounded preceding", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_RowsCurrentRow_ReturnsCorrectWindowFrame()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("rows current row");

        // Act
        var result = WindowFrameParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("rows current row", result.ToSqlWithoutCte());
    }
}
