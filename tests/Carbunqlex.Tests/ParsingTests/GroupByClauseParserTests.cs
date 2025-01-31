using Carbunqlex.Parsing;
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
}
