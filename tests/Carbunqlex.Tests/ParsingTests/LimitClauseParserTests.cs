using Carbunqlex.Lexing;
using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class LimitClauseParserTests
{
    public LimitClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void ParseLimit()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("limit 1");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("limit 1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseLimit_IgnoresRowsKeyword()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("limit 1 rows");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("limit 1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseLimit_IgnoresOnlyKeyword()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("limit 1 only");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("limit 1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseFetch_First()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("fetch first 10");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("fetch first 10", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseFetch_First_IgnoreRowsOnlyKeyword()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("fetch first 10 rows only");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("fetch first 10", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseFetch_Next()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("fetch next 5");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("fetch next 5", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseFetc_Next_IgnoreRowsOnlyKeyword()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("fetch next 5 rows only");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("fetch next 5", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseFetch_First_Percent()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("fetch first 10 percent");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("fetch first 10 percent", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseFetch_First_Percent_WithTies()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("fetch first 10 percent with ties");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("fetch first 10 percent with ties", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseFetch_Next_Percent()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("fetch next 5 percent");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("fetch next 5 percent", result.ToSqlWithoutCte());
    }

    [Fact]
    public void ParseFetch_Next_Percent_WithTies()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("fetch next 5 percent with ties");

        // Act
        var result = LimitClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.Equal("fetch next 5 percent with ties", result.ToSqlWithoutCte());
    }
}
