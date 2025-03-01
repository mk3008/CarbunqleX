using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
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

    //escape option
    [Fact]
    public void Parse_LikeExpressionWithEscapeOption_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("""
            'hello_world' like 'hello\_world' escape '\'
            """);
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<LikeExpression>(result);
        Assert.False(((LikeExpression)result).IsNegated);
        Assert.Equal("'hello_world'", ((LikeExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("""
            'hello\_world'
            """, ((LikeExpression)result).Right.ToSqlWithoutCte());
        Assert.Equal("""
            '\'
            """, ((LikeExpression)result).EscapeOption);
    }
}
