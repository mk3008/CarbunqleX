using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpressionParsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class CaseExpressionParserTests
{
    public CaseExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_CaseExpression_WithNoElse_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("case column when 1 then 'one' when 2 then 'two' end");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<CaseExpression>(result);
        Assert.Equal(2, ((CaseExpression)result).WhenClauses.Count);
        Assert.Equal("column", ((CaseExpression)result).CaseValue.ToSqlWithoutCte());
        Assert.Equal("1", ((CaseExpression)result).WhenClauses[0].WhenValue.ToSqlWithoutCte());
        Assert.Equal("'one'", ((CaseExpression)result).WhenClauses[0].ThenValue.ToSqlWithoutCte());
        Assert.Equal("2", ((CaseExpression)result).WhenClauses[1].WhenValue.ToSqlWithoutCte());
        Assert.Equal("'two'", ((CaseExpression)result).WhenClauses[1].ThenValue.ToSqlWithoutCte());
        Assert.Null(((CaseExpression)result).ElseValue);
    }

    [Fact]
    public void Parse_CaseExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("case column when 1 then 'one' when 2 then 'two' else 'other' end");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<CaseExpression>(result);
        Assert.Equal(2, ((CaseExpression)result).WhenClauses.Count);
        Assert.Equal("column", ((CaseExpression)result).CaseValue.ToSqlWithoutCte());
        Assert.Equal("1", ((CaseExpression)result).WhenClauses[0].WhenValue.ToSqlWithoutCte());
        Assert.Equal("'one'", ((CaseExpression)result).WhenClauses[0].ThenValue.ToSqlWithoutCte());
        Assert.Equal("2", ((CaseExpression)result).WhenClauses[1].WhenValue.ToSqlWithoutCte());
        Assert.Equal("'two'", ((CaseExpression)result).WhenClauses[1].ThenValue.ToSqlWithoutCte());
        Assert.Equal("'other'", ((CaseExpression)result).ElseValue?.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_CaseWhenExpression_WithNoElse_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("case when column = 1 then 'one' when column = 2 then 'two' end");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<CaseWhenExpression>(result);
        Assert.Equal(2, ((CaseWhenExpression)result).WhenClauses.Count);
        Assert.Equal("column = 1", ((CaseWhenExpression)result).WhenClauses[0].WhenValue.ToSqlWithoutCte());
        Assert.Equal("'one'", ((CaseWhenExpression)result).WhenClauses[0].ThenValue.ToSqlWithoutCte());
        Assert.Equal("column = 2", ((CaseWhenExpression)result).WhenClauses[1].WhenValue.ToSqlWithoutCte());
        Assert.Equal("'two'", ((CaseWhenExpression)result).WhenClauses[1].ThenValue.ToSqlWithoutCte());
        Assert.Null(((CaseWhenExpression)result).ElseValue?.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_CaseWhenExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("case when column = 1 then 'one' when column = 2 then 'two' else 'other' end");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<CaseWhenExpression>(result);
        Assert.Equal(2, ((CaseWhenExpression)result).WhenClauses.Count);
        Assert.Equal("column = 1", ((CaseWhenExpression)result).WhenClauses[0].WhenValue.ToSqlWithoutCte());
        Assert.Equal("'one'", ((CaseWhenExpression)result).WhenClauses[0].ThenValue.ToSqlWithoutCte());
        Assert.Equal("column = 2", ((CaseWhenExpression)result).WhenClauses[1].WhenValue.ToSqlWithoutCte());
        Assert.Equal("'two'", ((CaseWhenExpression)result).WhenClauses[1].ThenValue.ToSqlWithoutCte());
        Assert.Equal("'other'", ((CaseWhenExpression)result).ElseValue?.ToSqlWithoutCte());
    }
}
