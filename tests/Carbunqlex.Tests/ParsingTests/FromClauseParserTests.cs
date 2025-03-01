using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Clauses;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class FromClauseParserTests
{
    public FromClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("from table_a");
        // Act
        var result = FromClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("from table_a", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithAlias_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("from table_a as t1");
        // Act
        var result = FromClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("from table_a as t1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithTableAliasWithoutAs_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("from table_a t1");
        // Act
        var result = FromClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("from table_a as t1", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithJoin_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("from table_a as t1 inner join table_b as t2 on t1.column = t2.column");
        // Act
        var result = FromClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("from table_a as t1 inner join table_b as t2 on t1.column = t2.column", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithMultipleJoins_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("from table_a as t1 inner join table_b as t2 on t1.column = t2.column left join table_c as t3 on t1.column = t3.column");
        // Act
        var result = FromClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("from table_a as t1 inner join table_b as t2 on t1.column = t2.column left join table_c as t3 on t1.column = t3.column", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithColumnAlias_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("from table_a as t1 (column_a, column_b)");
        // Act
        var result = FromClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("from table_a as t1(column_a, column_b)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithOrdinality_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("from unnest(array['apple', 'banana', 'cherry']) with ordinality as t(value, id)");
        // Act
        var result = FromClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("from unnest(array['apple', 'banana', 'cherry']) with ordinality as t(value, id)", result.ToSqlWithoutCte());
    }

    // comma separated tables
    [Fact]
    public void Parse_WithCommaSeparatedTables_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("from table_a as t1, table_b as t2");
        // Act
        var result = FromClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("from table_a as t1 cross join table_b as t2", result.ToSqlWithoutCte());
    }
}
