using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpressionParsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class ValueParserTests
{
    public ValueParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_BetweenExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("'test'");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

    }
}
