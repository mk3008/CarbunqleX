using Carbunqlex.Lexing;
using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class OverClauseParserTests
{
    public OverClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_OverClause_ReturnsCorrectOverClause()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("over()");

        // Act
        var result = OverClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("over()", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_OverClauseWithWindowFunction_ReturnsCorrectOverClause()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("over(partition by a.value order by a.id rows between unbounded preceding and current row)");

        // Act
        var result = OverClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("over(partition by a.value order by a.id rows between unbounded preceding and current row)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_OverClauseWithPartitionBy_ReturnsCorrectOverClause()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("over(partition by a.value)");

        // Act
        var result = OverClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("over(partition by a.value)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_OverClauseWithOrderBy_ReturnsCorrectOverClause()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("over(order by a.id)");

        // Act
        var result = OverClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("over(order by a.id)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_OverClauseWithPartitionByAndOrderBy_ReturnsCorrectOverClause()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("over(partition by a.value order by a.id)");

        // Act
        var result = OverClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal("over(partition by a.value order by a.id)", result.ToSqlWithoutCte());
    }
}
