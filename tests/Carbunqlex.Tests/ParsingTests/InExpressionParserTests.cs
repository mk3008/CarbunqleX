using Carbunqlex.Parsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class InExpressionParserTests
{
    public InExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_InExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column in (1, 2, 3)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<InExpression>(result);
        Assert.False(((InExpression)result).IsNegated);
        Assert.Equal("column", ((InExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("1, 2, 3", ((InExpression)result).Right.ToSqlWithoutCte());
        Assert.Equal("column in (1, 2, 3)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_NotInExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column not in (1, 2, 3)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<InExpression>(result);
        Assert.True(((InExpression)result).IsNegated);
        Assert.Equal("column", ((InExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("1, 2, 3", ((InExpression)result).Right.ToSqlWithoutCte());
        Assert.Equal("column not in (1, 2, 3)", result.ToSqlWithoutCte());
    }
}
