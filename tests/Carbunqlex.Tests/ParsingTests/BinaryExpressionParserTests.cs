using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpressionParsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class BinaryExpressionParserTests
{
    public BinaryExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_AdditionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("1 + 2");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BinaryExpression>(result);
        Assert.Equal("1", ((BinaryExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("+", ((BinaryExpression)result).Operator);
        Assert.Equal("2", ((BinaryExpression)result).Right.ToSqlWithoutCte());
        Assert.Equal("1 + 2", result.ToSqlWithoutCte());
    }
}
