using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
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

    [Fact]
    public void Parse_CountAllFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("count(all column)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("count", ((FunctionExpression)result).FunctionName);
        Assert.Equal("column", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("count(column)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_DistinctCountFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("count(distinct column)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("count", ((FunctionExpression)result).FunctionName);
        Assert.Equal("distinct", ((FunctionExpression)result).PrefixModifier);
        Assert.Equal("column", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("count(distinct column)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_OverClauseFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("count(*) over(partition by column)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("count", ((FunctionExpression)result).FunctionName);
        Assert.Equal("*", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("over(partition by column)", ((FunctionExpression)result).FunctionModifier!.ToSqlWithoutCte());
        Assert.Equal("count(*) over(partition by column)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_FilterClauseFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("count(*) filter(where column > 0)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("count", ((FunctionExpression)result).FunctionName);
        Assert.Equal("*", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("filter(where column > 0)", ((FunctionExpression)result).FunctionModifier!.ToSqlWithoutCte());
        Assert.Equal("count(*) filter(where column > 0)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_WithinGroupClauseFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("array_agg(column) within group(order by column)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("array_agg", ((FunctionExpression)result).FunctionName);
        Assert.Equal("column", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("within group(order by column)", ((FunctionExpression)result).FunctionModifier!.ToSqlWithoutCte());
        Assert.Equal("array_agg(column) within group(order by column)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_FilterOverClauseFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("count(*) filter(where column > 0) over(partition by column)");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("count", ((FunctionExpression)result).FunctionName);
        Assert.Equal("*", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("filter(where column > 0) over(partition by column)", ((FunctionExpression)result).FunctionModifier!.ToSqlWithoutCte());
        Assert.Equal("count(*) filter(where column > 0) over(partition by column)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_NamedWindowDefinitionFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("count(*) over window_name");

        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FunctionExpression>(result);
        Assert.Equal("count", ((FunctionExpression)result).FunctionName);
        Assert.Equal("*", ((FunctionExpression)result).Arguments.ToSqlWithoutCte());
        Assert.Equal("over window_name", ((FunctionExpression)result).FunctionModifier!.ToSqlWithoutCte());
        Assert.Equal("count(*) over window_name", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_NormalizeFunctionExpression_ReturnsCorrectExpression()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("normalize(U&'\\FB01', nfkd)");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        Assert.NotNull(result);
        Assert.IsType<NormalizeExpression>(result);
        Assert.Equal("normalize(U&'\\FB01', nfkd)", result.ToSqlWithoutCte());
    }

    [Fact]
    public void Parse_Trim()
    {
        // Arrange
        var tokenizer = new SqlTokenizer("trim('  yxTomxx  ')");
        // Act
        var result = ValueExpressionParser.Parse(tokenizer);
        Output.WriteLine(result.ToSqlWithoutCte());
        // Assert
        Assert.NotNull(result);
        Assert.IsType<TrimExpression>(result);
        Assert.Equal("trim('  yxTomxx  ')", result.ToSqlWithoutCte());
    }
}
