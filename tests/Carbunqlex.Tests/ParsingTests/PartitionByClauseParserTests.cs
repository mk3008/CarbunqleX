using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class PartitionByClauseParserTests
{
    public PartitionByClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_ValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("partition by a.id");

        // Act
        var result = PartitionByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.PartitionByColumns);
        Assert.Equal("a.id", result.PartitionByColumns[0].ToSqlWithoutCte());
        Assert.Equal("partition by a.id", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithMultipleColumns_ReturnsExpectedResult()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("partition by a.id, b.value");

        // Act
        var result = PartitionByClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PartitionByColumns.Count);
        Assert.Equal("a.id", result.PartitionByColumns[0].ToSqlWithoutCte());
        Assert.Equal("b.value", result.PartitionByColumns[1].ToSqlWithoutCte());
        Assert.Equal("partition by a.id, b.value", result.ToSqlWithoutCte());
    }
}
