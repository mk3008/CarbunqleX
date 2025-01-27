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
    public void Parse_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("'test'");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_HandlesIsNullExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column is null");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        Assert.Equal("column is null", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_HandlesIsNotNullExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column is not null");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        Assert.Equal("column is not null", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_HandlesParameterExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column = :value");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        Assert.Equal("column = :value", result.ToSqlWithoutCte());
    }
}
