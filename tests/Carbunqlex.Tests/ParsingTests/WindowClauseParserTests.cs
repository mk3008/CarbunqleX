using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Clauses;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class WindowClauseParserTests
{
    public WindowClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_SingleWindow()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("window w as (partition by column1 order by column2)");
        // Act
        var result = WindowClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("window w as (partition by column1 order by column2)", result.ToSqlWithoutCte());
    }

    //multiple
    [Fact]
    public void Parse_WithMultipleWindows()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("window w1 as (partition by column1 order by column2), w2 as (partition by column3 order by column4)");
        // Act
        var result = WindowClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("window w1 as (partition by column1 order by column2), w2 as (partition by column3 order by column4)", result.ToSqlWithoutCte());
    }
}
