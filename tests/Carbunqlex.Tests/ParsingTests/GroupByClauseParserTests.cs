using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Clauses;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class GroupByClauseParserTests
{
    public GroupByClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_WithSingleColumn_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("group by a.id");
        // Act
        var result = GroupByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.GroupByColumns);
        Assert.Equal("a.id", result.GroupByColumns[0].ToSqlWithoutCte());
        Assert.Equal("group by a.id", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithMultipleColumns_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("group by a.id, b.value");
        // Act
        var result = GroupByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.GroupByColumns.Count);
        Assert.Equal("a.id", result.GroupByColumns[0].ToSqlWithoutCte());
        Assert.Equal("b.value", result.GroupByColumns[1].ToSqlWithoutCte());
        Assert.Equal("group by a.id, b.value", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithCube_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("group by cube(a.id, b.value)");
        // Act
        var result = GroupByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("group by cube(a.id, b.value)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithRollup_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("group by rollup(a.id, b.value)");
        // Act
        var result = GroupByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("group by rollup(a.id, b.value)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithGroupingSets_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("group by grouping sets((a.id, b.value), (a.id), (b.value))");
        // Act
        var result = GroupByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("group by grouping sets((a.id, b.value), (a.id), (b.value))", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithComplexGroupingSets_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("group by a, cube(b, c), rollup(d, e), grouping sets((b, d), (c, e))");
        // Act
        var result = GroupByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("group by a, cube(b, c), rollup(d, e), grouping sets((b, d), (c, e))", result.ToSqlWithoutCte());
    }
}
