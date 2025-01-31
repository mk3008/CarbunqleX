using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class JoinClauseParserTests
{
    public JoinClauseParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_InnerJoinClause_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("inner join table_a as t2 on t1.column = t2.column");

        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        Assert.Equal("inner join table_a as t2 on t1.column = t2.column", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_LeftJoinClause_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("left join table_a as t2 on t1.column = t2.column");
        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("left join table_a as t2 on t1.column = t2.column", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_RightJoinClause_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("right join table_a as t2 on t1.column = t2.column");
        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("right join table_a as t2 on t1.column = t2.column", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_FullJoinClause_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("full join table_a as t2 on t1.column = t2.column");
        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("full join table_a as t2 on t1.column = t2.column", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_CrossJoinClause_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("cross join table_a as t2");
        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("cross join table_a as t2", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_CrossJoinClauseWithComma_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer(", table_a as t2");
        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("cross join table_a as t2", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_LeftJoinClauseWithLateral_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("left join lateral get_product_names(m.id) as pname on true");
        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("left join lateral get_product_names(m.id) as pname on true", result.ToSqlWithoutCte());
    }

    //column alias
    [Fact]
    public void Parse_InnerJoinClauseWithColumnAlias_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("inner join table_a as t2(column1, column2) on t1.column = t2.column");
        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("inner join table_a as t2(column1, column2) on t1.column = t2.column", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_LeftOuterJoinClause_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("left outer join table_a as t2 on t1.column = t2.column");
        // Act
        var result = JoinClauseParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.Equal("left outer join table_a as t2 on t1.column = t2.column", result.ToSqlWithoutCte());
    }
}
