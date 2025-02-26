using Carbunqlex.Lexing;
using Carbunqlex.Parsing;
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

    [Fact]
    public void Parse_HandlesUnicodeEscape()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("""
            U&'d!0061t!+000061' uescape '!'
            """);
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("""
            U&'d!0061t!+000061' uescape '!'
            """, result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_HandlesHexString()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("""
            X'68656C6C6F'
            """);
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("""
            X'68656C6C6F'
            """, result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_HandlesBitString()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("""
            B'1010101'
            """);
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("""
            B'1010101'
            """, result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_HandlesEscapeString()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("""
            E'Hello\nWorld'
            """);
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("""
            E'Hello\nWorld'
            """, result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_Extract()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("extract(year from birthdate)");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("extract(year from birthdate)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_Wildcard()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("table_a.*");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        Assert.Equal("table_a.*", result.ToSqlWithoutCte());
    }
}
