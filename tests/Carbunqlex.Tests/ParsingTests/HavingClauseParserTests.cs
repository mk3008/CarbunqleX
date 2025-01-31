using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class HavingClauseParserTests
{
    public HavingClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_WithSingleColumn_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("having a.id = 1");
        // Act
        var result = HavingClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("having a.id = 1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithAndOperator_ReturnsCorrectSql()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("having a.id = 1 and b.value = 2");
        // Act
        var result = HavingClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.Equal("having a.id = 1 and b.value = 2", result.ToSqlWithoutCte());
    }
}
