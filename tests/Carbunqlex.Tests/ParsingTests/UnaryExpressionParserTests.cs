using Carbunqlex.Parsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class UnaryExpressionParserTests
{
    public UnaryExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_NegativeInfinityConstantExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("-infinity");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<LiteralExpression>(result);
        Assert.Equal("-infinity", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_UnaryExpressionWithMinusOperator_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("- 1");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<UnaryExpression>(result);
        Assert.Equal("-", ((UnaryExpression)result).Operator);
        Assert.Equal("1", ((UnaryExpression)result).Operand.ToSqlWithoutCte());
        Assert.Equal("- 1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_UnaryExpressionWithNotOperator_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("not true");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<UnaryExpression>(result);
        Assert.Equal("not", ((UnaryExpression)result).Operator);
        Assert.Equal("true", ((UnaryExpression)result).Operand.ToSqlWithoutCte());
        Assert.Equal("not true", result.ToSqlWithoutCte());
    }
}
