using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class WhereParserTests
{
    public WhereParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_WithSingleColumn_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("where a.id = 1");
        // Act
        var result = WhereClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("where a.id = 1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithAndOperator_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("where a.id = 1 and b.value = 2");
        // Act
        var result = WhereClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("where a.id = 1 and b.value = 2", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithOrOperator_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("where a.id = 1 or b.value = 2");
        // Act
        var result = WhereClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("where a.id = 1 or b.value = 2", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithInOperator_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("where a.id in (1, 2, 3)");
        // Act
        var result = WhereClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("where a.id in (1, 2, 3)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithLikeOperator_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("where a.id like '%test%'");
        // Act
        var result = WhereClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("where a.id like '%test%'", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithBetweenOperator_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("where a.id between 1 and 10");
        // Act
        var result = WhereClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("where a.id between 1 and 10", result.ToSqlWithoutCte());
    }
}
