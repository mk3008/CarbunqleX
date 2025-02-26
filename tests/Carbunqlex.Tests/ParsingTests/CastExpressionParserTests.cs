using Carbunqlex.Lexing;
using Carbunqlex.Parsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class CastExpressionParserTests
{
    public CastExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_CastExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("cast(column as int)");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<CastExpression>(result);
        Assert.Equal("column", ((CastExpression)result).Expression.ToSqlWithoutCte());
        Assert.Equal("int", ((CastExpression)result).TargetType);
        Assert.Equal("cast(column as int)", result.ToSqlWithoutCte());
    }
}
