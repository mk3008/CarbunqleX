using Carbunqlex.Parsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class ParenthesizedExpressionParserTests
{
    public ParenthesizedExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_WithParentheses()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("(1 + 2)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ParenthesizedExpression>(result);
        Assert.Equal("(1 + 2)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithNegativeNumber()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("(-1)");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<ParenthesizedExpression>(result);
        Assert.Equal("(- 1)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithUnaryExpressionAndParentheses()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("-(1+2)");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<UnaryExpression>(result);
        Assert.Equal("- (1 + 2)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithNestedParentheses()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("((1 + 2))");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<ParenthesizedExpression>(result);
        Assert.Equal("((1 + 2))", result.ToSqlWithoutCte());
    }
}
