using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpressionParsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class ColumnExpressionParserTests
{
    public ColumnExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_SingleColumn_ReturnsColumnExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("columnName");

        // Act
        var result = ColumnExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("columnName", result.ColumnName);
        Assert.Empty(result.Namespaces);
    }

    [Fact]
    public void Parse_ColumnWithNamespace_ReturnsColumnExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("schemaName.tableName.columnName");

        // Act
        var result = ColumnExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("columnName", result.ColumnName);
        Assert.Equal(new List<string> { "schemaName", "tableName" }, result.Namespaces);
    }

    [Fact]
    public void Parse_InvalidToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("123InvalidToken");

        // Act & Assert
        Assert.Throws<SqlParsingException>(() => ColumnExpressionParser.Parse(tokenizer));
    }

    [Fact]
    public void Parse_EmptyInput_ThrowsInvalidOperationException()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("");

        // Act & Assert
        Assert.Throws<SqlParsingException>(() => ColumnExpressionParser.Parse(tokenizer));
    }

    [Fact]
    public void Parse_UnexpectedEndOfInput_ThrowsInvalidOperationException()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("schemaName.");

        // Act & Assert
        Assert.Throws<SqlParsingException>(() => ColumnExpressionParser.Parse(tokenizer));
    }
}
