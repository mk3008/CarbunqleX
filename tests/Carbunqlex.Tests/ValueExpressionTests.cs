namespace Carbunqlex.Tests;

using Carbunqlex.QueryModels;
using Xunit;
using Xunit.Abstractions;

public class ValueExpressionTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ConstantValue_ToSql_ReturnsValueAsString()
    {
        var constant = new ConstantExpression(42);
        var sql = constant.ToSql();
        output.WriteLine(sql);
        Assert.Equal("42", sql);
    }

    [Fact]
    public void UnaryExpression_ToSql_ReturnsCorrectSql()
    {
        var operand = new ConstantExpression(42);
        var unary = new UnaryExpression("-", operand);
        var sql = unary.ToSql();
        output.WriteLine(sql);
        Assert.Equal("- 42", sql);
    }

    [Fact]
    public void UnaryExpression_NotOperator_ReturnsCorrectSql()
    {
        var operand = new ConstantExpression(true);
        var unary = new UnaryExpression("not", operand);
        var sql = unary.ToSql();
        output.WriteLine(sql);
        Assert.Equal("not True", sql);
    }

    [Fact]
    public void BinaryExpression_ToSql_ReturnsCorrectSql()
    {
        var left = new ConstantExpression(42);
        var right = new ConstantExpression(24);
        var binary = new BinaryExpression("+", left, right);
        var sql = binary.ToSql();
        output.WriteLine(sql);
        Assert.Equal("42 + 24", sql);
    }

    [Fact]
    public void FunctionExpression_ToSql_ReturnsCorrectSql()
    {
        var argument = new ConstantExpression(42);
        var function = new FunctionExpression("ABS", new[] { argument });
        var sql = function.ToSql();
        output.WriteLine(sql);
        Assert.Equal("ABS(42)", sql);
    }

    [Fact]
    public void ColumnExpression_ToSql_ReturnsCorrectSql()
    {
        var column = new ColumnExpression("TableName", "ColumnName");
        var sql = column.ToSql();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName", sql);
    }

    [Fact]
    public void BetweenExpression_ToSql_ReturnsCorrectSql()
    {
        var left = new ConstantExpression(10);
        var start = new ConstantExpression(5);
        var end = new ConstantExpression(15);
        var between = new BetweenExpression(left, false, start, end);
        var sql = between.ToSql();
        output.WriteLine(sql);
        Assert.Equal("10 between 5 and 15", sql);
    }

    [Fact]
    public void BetweenExpression_NotBetween_ToSql_ReturnsCorrectSql()
    {
        var left = new ConstantExpression(10);
        var start = new ConstantExpression(5);
        var end = new ConstantExpression(15);
        var between = new BetweenExpression(left, true, start, end);
        var sql = between.ToSql();
        output.WriteLine(sql);
        Assert.Equal("10 not between 5 and 15", sql);
    }

    [Fact]
    public void InExpression_ToSql_ReturnsCorrectSql()
    {
        var left = new ColumnExpression("TableName", "ColumnName");
        var right1 = new ConstantExpression(1);
        var right2 = new ConstantExpression(2);
        var inExpression = new InExpression(left, false, right1, right2);
        var sql = inExpression.ToSql();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName in (1, 2)", sql);
    }

    [Fact]
    public void InExpression_NotIn_ToSql_ReturnsCorrectSql()
    {
        var left = new ColumnExpression("TableName", "ColumnName");
        var right1 = new ConstantExpression(1);
        var right2 = new ConstantExpression(2);
        var inExpression = new InExpression(left, true, right1, right2);
        var sql = inExpression.ToSql();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName not in (1, 2)", sql);
    }

    [Fact]
    public void ParenthesizedExpression_ComplexExpression_ReturnsCorrectSql()
    {
        var left = new ConstantExpression(1);
        var right = new ConstantExpression(2);
        var addition = new BinaryExpression("+", left, right);
        var parenthesizedAddition = new ParenthesizedExpression(addition);
        var three = new ConstantExpression(3);
        var multiplication = new BinaryExpression("*", parenthesizedAddition, three);
        var sql = multiplication.ToSql();
        output.WriteLine(sql);
        Assert.Equal("(1 + 2) * 3", sql);
    }

    [Fact]
    public void LikeExpression_ToSql_ReturnsCorrectSql()
    {
        var left = new ColumnExpression("TableName", "ColumnName");
        var right = ConstantExpression.CreateEscapeString("%value%");
        var likeExpression = new LikeExpression(left, false, right);
        var sql = likeExpression.ToSql();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName like '%value%'", sql);
    }

    [Fact]
    public void LikeExpression_NotLike_ToSql_ReturnsCorrectSql()
    {
        var left = new ColumnExpression("TableName", "ColumnName");
        var right = ConstantExpression.CreateEscapeString("%value%");
        var likeExpression = new LikeExpression(left, true, right);
        var sql = likeExpression.ToSql();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName not like '%value%'", sql);
    }

    [Fact]
    public void ConstantExpression_CreateEscapeString_ReturnsEscapedConstantExpression()
    {
        var input = "O'Reilly";
        var expected = "'O''Reilly'";
        var constantExpression = ConstantExpression.CreateEscapeString(input);
        var actual = constantExpression.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CaseExpression_CaseToSql_ReturnsCorrectSql()
    {
        var when1 = new ConstantExpression(1);
        var then1 = ConstantExpression.CreateEscapeString("One");
        var when2 = new ConstantExpression(2);
        var then2 = ConstantExpression.CreateEscapeString("Two");
        var elseExpr = ConstantExpression.CreateEscapeString("Other");

        var caseExpression = new CaseExpression(
            new List<WhenThenPair>
            {
                new WhenThenPair(when1, then1),
                new WhenThenPair(when2, then2)
            },
            elseExpr
        );

        var sql = caseExpression.ToSql();
        output.WriteLine(sql);
        Assert.Equal("case when 1 then 'One' when 2 then 'Two' else 'Other' end", sql);
    }

    [Fact]
    public void CaseExpression_CaseWhenToSql_ReturnsCorrectSql()
    {
        var caseExpr = new ColumnExpression("TableName", "ColumnName");
        var when1 = new ConstantExpression(1);
        var then1 = ConstantExpression.CreateEscapeString("One");
        var when2 = new ConstantExpression(2);
        var then2 = ConstantExpression.CreateEscapeString("Two");
        var elseExpr = ConstantExpression.CreateEscapeString("Other");

        var caseExpression = new CaseExpression(
            caseExpr,
            new List<WhenThenPair>
            {
                new WhenThenPair(when1, then1),
                new WhenThenPair(when2, then2)
            },
            elseExpr
        );

        var sql = caseExpression.ToSql();
        output.WriteLine(sql);
        Assert.Equal("case TableName.ColumnName when 1 then 'One' when 2 then 'Two' else 'Other' end", sql);
    }
}
