using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpression;
using Carbunqlex.ValueExpressions;
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
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ColumnExpression>(result);
        Assert.Equal("columnName", ((ColumnExpression)result).ColumnName);
        Assert.Empty(((ColumnExpression)result).Namespaces);
    }

    [Fact]
    public void Parse_ColumnWithNamespace_ReturnsColumnExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("schemaName.tableName.columnName");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ColumnExpression>(result);
        Assert.Equal("columnName", ((ColumnExpression)result).ColumnName);
        Assert.Equal(new List<string> { "schemaName", "tableName" }, ((ColumnExpression)result).Namespaces);
    }

    [Fact]
    public void Parse_UnexpectedEndOfInput_ThrowsInvalidOperationException()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("schemaName.");

        // Act & Assert
        Assert.Throws<SqlParsingException>(() => ValueExpressionParser.Parse(tokenizer));
    }
}
