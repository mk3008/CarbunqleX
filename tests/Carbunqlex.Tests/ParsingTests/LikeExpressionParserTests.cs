using Carbunqlex.Parsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class LikeExpressionParserTests
{
    public LikeExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_LikeExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column like 'a%'");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LikeExpression>(result);
        Assert.False(((LikeExpression)result).IsNegated);
        Assert.Equal("column", ((LikeExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("'a%'", ((LikeExpression)result).Right.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_NotLikeExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column not like 'a%'");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LikeExpression>(result);
        Assert.True(((LikeExpression)result).IsNegated);
        Assert.Equal("column", ((LikeExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("'a%'", ((LikeExpression)result).Right.ToSqlWithoutCte());
    }
}
