using Carbunqlex.Parsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class ArrayExpressionParserTests
{
    public ArrayExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_ArrayExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("array[1, 2, 3]");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ArrayExpression>(result);
        Assert.Equal("1, 2, 3", ((ArrayExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("array[1, 2, 3]", result.ToSqlWithoutCte());
    }
}
