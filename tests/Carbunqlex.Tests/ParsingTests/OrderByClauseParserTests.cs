using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class OrderByClauseParserTests
{
    public OrderByClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_WithSingleColumn_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("order by a.id");

        // Act
        var result = OrderByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.OrderByColumns);
        Assert.Equal("a.id", result.OrderByColumns[0].ToSqlWithoutCte());
        Assert.Equal("order by a.id", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithMultipleColumns_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("order by a.id, b.value");

        // Act
        var result = OrderByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.OrderByColumns.Count);
        Assert.Equal("a.id", result.OrderByColumns[0].ToSqlWithoutCte());
        Assert.Equal("b.value", result.OrderByColumns[1].ToSqlWithoutCte());
        Assert.Equal("order by a.id, b.value", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithMultipleColumnsAndDirection_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("order by a.id asc, b.value desc");

        // Act
        var result = OrderByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.OrderByColumns.Count);
        Assert.Equal("a.id", result.OrderByColumns[0].ToSqlWithoutCte());
        Assert.Equal("b.value desc", result.OrderByColumns[1].ToSqlWithoutCte());
        Assert.Equal("order by a.id, b.value desc", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithMultipleColumnsAndDirectionAndNullsOrder_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("order by a.id asc nulls first, b.value desc nulls last");

        // Act
        var result = OrderByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.OrderByColumns.Count);
        Assert.Equal("a.id nulls first", result.OrderByColumns[0].ToSqlWithoutCte());
        Assert.Equal("b.value desc nulls last", result.OrderByColumns[1].ToSqlWithoutCte());
        Assert.Equal("order by a.id nulls first, b.value desc nulls last", result.ToSqlWithoutCte());
    }
}
