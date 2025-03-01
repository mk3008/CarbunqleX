using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class SelectExpressionParserTests
{
    public SelectExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_WithAlias()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column as alias");
        // Act
        var result = SelectExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("alias", result.Alias);
        Assert.Equal("column as alias", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithoutAlias()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column");
        // Act
        var result = SelectExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("column", result.Alias);
        Assert.Equal("column", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithAliasAndNoAs()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column alias");
        // Act
        var result = SelectExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("alias", result.Alias);
        Assert.Equal("column as alias", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithCommaSeparatedColumns()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column1, column2");
        // Act
        var result = SelectExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("column1", result.Alias);
        Assert.Equal("column1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithFromKeyword()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column from table");
        // Act
        var result = SelectExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("column", result.Alias);
        Assert.Equal("column", result.ToSqlWithoutCte());
    }
}
