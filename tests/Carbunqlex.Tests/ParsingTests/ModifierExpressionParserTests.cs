using Carbunqlex.Parsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class ModifierExpressionParserTests
{
    public ModifierExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_InExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("interval '1 day'");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ModifierExpression>(result);
        Assert.Equal("interval", ((ModifierExpression)result).Modifier);
        Assert.Equal("'1 day'", ((ModifierExpression)result).Value.ToSqlWithoutCte());
        Assert.Equal("interval '1 day'", result.ToSqlWithoutCte());
    }
}
