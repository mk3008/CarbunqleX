using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class InExpressionParserTests
{
    public InExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_InExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column in (1, 2, 3)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<InExpression>(result);
        Assert.False(((InExpression)result).IsNegated);
        Assert.Equal("column", ((InExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("(1, 2, 3)", ((InExpression)result).Right.ToSqlWithoutCte());
        Assert.Equal("column in (1, 2, 3)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_NotInExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("column not in (1, 2, 3)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<InExpression>(result);
        Assert.True(((InExpression)result).IsNegated);
        Assert.Equal("column", ((InExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("(1, 2, 3)", ((InExpression)result).Right.ToSqlWithoutCte());
        Assert.Equal("column not in (1, 2, 3)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_InExpression_WithSubquery_ReturnsCorrectExpression_SingleColumn()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("a.c1 in (select b.c1 from table_b as b)");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<InExpression>(result);
        Assert.False(((InExpression)result).IsNegated);
        Assert.Equal("a.c1", ((InExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("(select b.c1 from table_b as b)", ((InExpression)result).Right.ToSqlWithoutCte());
        Assert.Equal("a.c1 in (select b.c1 from table_b as b)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_InExpression_WithSubquery_ReturnsCorrectExpression_MultipleColumns()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("(a.c1, a.c2) in (select b.c1, b.c2 from table_b as b)");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<InExpression>(result);
        Assert.False(((InExpression)result).IsNegated);
        Assert.Equal("(a.c1, a.c2)", ((InExpression)result).Left.ToSqlWithoutCte());
        Assert.Equal("(select b.c1, b.c2 from table_b as b)", ((InExpression)result).Right.ToSqlWithoutCte());
        Assert.Equal("(a.c1, a.c2) in (select b.c1, b.c2 from table_b as b)", result.ToSqlWithoutCte());
    }
}
