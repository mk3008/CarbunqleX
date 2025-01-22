using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpressionParsing;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class FunctionExpressionParserTests
{
    public FunctionExpressionParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void Parse_CountFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("count(*)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("count", ((FunctionExpression)result).FunctionName);
        Assert.Equal("*", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("count(*)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_CoalesceFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("coalesce(column, 0)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("coalesce", ((FunctionExpression)result).FunctionName);
        Assert.Equal("column, 0", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("coalesce(column, 0)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_ArrayAggFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("array_agg(value order by sort_column)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("array_agg", ((FunctionExpression)result).FunctionName);
        Assert.Equal("value order by sort_column", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("array_agg(value order by sort_column)", result.ToSqlWithoutCte());
    }
}
