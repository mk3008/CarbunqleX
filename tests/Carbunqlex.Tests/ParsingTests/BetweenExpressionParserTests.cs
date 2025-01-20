using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpressionParsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class BetweenExpressionParserTests
{
    public BetweenExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_BetweenExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column between 1 and 10");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BetweenExpression>(result);
        Assert.False(((BetweenExpression)result).IsNegated);
        Assert.Equal("column", ((BetweenExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("1", ((BetweenExpression)result).Start.ToSqlWithoutCte());
        Assert.Equal("10", ((BetweenExpression)result).End.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_NotBetweenExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column not between 1 and 10");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BetweenExpression>(result);
        Assert.True(((BetweenExpression)result).IsNegated);
        Assert.Equal("column", ((BetweenExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("1", ((BetweenExpression)result).Start.ToSqlWithoutCte());
        Assert.Equal("10", ((BetweenExpression)result).End.ToSqlWithoutCte());
    }
}
