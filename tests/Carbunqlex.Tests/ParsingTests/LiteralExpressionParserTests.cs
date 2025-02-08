using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpression;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class LiteralExpressionParserTests
{
    public LiteralExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_ValidIntegerToken_ReturnsConstantExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("123");
        var expectedValue = "123";

        // Act
        var result = LiteralExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LiteralExpression>(result);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Parse_ValidFloatToken_ReturnsConstantExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("123.456");
        var expectedValue = "123.456";

        // Act
        var result = LiteralExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LiteralExpression>(result);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Parse_ValidTrueToken_ReturnsConstantExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("true");
        var expectedValue = "true";

        // Act
        var result = LiteralExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LiteralExpression>(result);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Parse_ValidFalseToken_ReturnsConstantExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("false");
        var expectedValue = "false";

        // Act
        var result = LiteralExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LiteralExpression>(result);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Parse_ValidNullToken_ReturnsConstantExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("null");
        var expectedValue = "null";

        // Act
        var result = LiteralExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LiteralExpression>(result);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Parse_InvalidToken_ThrowsNotSupportedException()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("SELECT");

        // Act & Assert
        var exception = Assert.Throws<SqlParsingException>(() => LiteralExpressionParser.Parse(tokenizer));
        Assert.Equal("Unexpected token type encountered. Expected: Literal, Actual: Command(SELECT), Position: 6", exception.Message);
    }

    [Fact]
    public void Parse_EndOfInput_ThrowsInvalidOperationException()
    {
        // Arrange
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            var tokenizer = new SqlTokenizer("");
        });
        Assert.Equal("The value cannot be an empty string or composed entirely of whitespace. (Parameter 'sql')", exception.Message);
    }
}
