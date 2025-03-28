﻿using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ValueExpressionTests;

public class ValueExpressionTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ConstantValue_ToSql_ReturnsValueAsString()
    {
        var constant = new LiteralExpression(42);
        var sql = constant.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("42", sql);
    }

    [Fact]
    public void UnaryExpression_ToSql_ReturnsCorrectSql()
    {
        var operand = new LiteralExpression(42);
        var unary = new UnaryExpression("-", operand);
        var sql = unary.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("-42", sql);
    }

    [Fact]
    public void UnaryExpression_NotOperator_ReturnsCorrectSql()
    {
        var operand = new LiteralExpression(true);
        var unary = new UnaryExpression("not", operand);
        var sql = unary.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("not True", sql);
    }

    [Fact]
    public void BinaryExpression_ToSql_ReturnsCorrectSql()
    {
        var left = new LiteralExpression(42);
        var right = new LiteralExpression(24);
        var binary = new BinaryExpression("+", left, right);
        var sql = binary.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("42 + 24", sql);
    }

    [Fact]
    public void FunctionExpression_ToSql_ReturnsCorrectSql()
    {
        var argument = new LiteralExpression(42);
        var function = ValueBuilder.Function("ABS", new[] { argument });
        var sql = function.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("ABS(42)", sql);
    }

    [Fact]
    public void ColumnExpression_ToSql_ReturnsCorrectSql()
    {
        var column = new ColumnExpression("TableName", "ColumnName");
        var sql = column.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName", sql);
    }

    [Fact]
    public void BetweenExpression_ToSql_ReturnsCorrectSql()
    {
        var left = new LiteralExpression(10);
        var start = new LiteralExpression(5);
        var end = new LiteralExpression(15);
        var between = ValueBuilder.Between(left, start, end);
        var sql = between.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("10 between 5 and 15", sql);
    }

    [Fact]
    public void BetweenExpression_NotBetween_ToSql_ReturnsCorrectSql()
    {
        var left = new LiteralExpression(10);
        var start = new LiteralExpression(5);
        var end = new LiteralExpression(15);
        var between = ValueBuilder.NotBetween(left, start, end);
        var sql = between.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("10 not between 5 and 15", sql);
    }

    [Fact]
    public void InExpression_ToSql_ReturnsCorrectSql()
    {
        var left = new ColumnExpression("TableName", "ColumnName");
        var right1 = new LiteralExpression(1);
        var right2 = new LiteralExpression(2);
        var inExpression = ValueBuilder.In(new InValueGroupExpression(left), new InValueGroupExpression(right1, right2));
        var sql = inExpression.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName in (1, 2)", sql);
    }

    [Fact]
    public void InExpression_NotIn_ToSql_ReturnsCorrectSql()
    {
        var left = new ColumnExpression("TableName", "ColumnName");
        var right1 = new LiteralExpression(1);
        var right2 = new LiteralExpression(2);
        var inExpression = ValueBuilder.NotIn(new InValueGroupExpression(left), new InValueGroupExpression(right1, right2));
        var sql = inExpression.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName not in (1, 2)", sql);
    }

    [Fact]
    public void ParenthesizedExpression_ComplexExpression_ReturnsCorrectSql()
    {
        var left = new LiteralExpression(1);
        var right = new LiteralExpression(2);
        var addition = new BinaryExpression("+", left, right);
        var parenthesizedAddition = new ParenthesizedExpression(addition);
        var three = new LiteralExpression(3);
        var multiplication = new BinaryExpression("*", parenthesizedAddition, three);
        var sql = multiplication.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("(1 + 2) * 3", sql);
    }

    [Fact]
    public void LikeExpression_ToSql_ReturnsCorrectSql()
    {
        var left = new ColumnExpression("TableName", "ColumnName");
        var right = ValueBuilder.Constant("'%value%'");
        var likeExpression = ValueBuilder.Like(left, right);
        var sql = likeExpression.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName like '%value%'", sql);
    }

    [Fact]
    public void LikeExpression_NotLike_ToSql_ReturnsCorrectSql()
    {
        var left = new ColumnExpression("TableName", "ColumnName");
        var right = ValueBuilder.Constant("'%value%'");
        var likeExpression = ValueBuilder.NotLike(left, right);
        var sql = likeExpression.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("TableName.ColumnName not like '%value%'", sql);
    }

    [Fact]
    public void ConstantExpression_CreateEscapeString_ReturnsEscapedConstantExpression()
    {
        var input = "'O''Reilly'";
        var expected = "'O''Reilly'";
        var constantExpression = ValueBuilder.Constant(input);
        var actual = constantExpression.ToSqlWithoutCte();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CaseExpressionWithoutCase_ToSql_ReturnsCorrectSql()
    {
        var when1 = new LiteralExpression(1);
        var then1 = ValueBuilder.Constant("'One'");
        var when2 = new LiteralExpression(2);
        var then2 = ValueBuilder.Constant("'Two'");
        var elseExpr = ValueBuilder.Constant("'Other'");

        var caseExpression = new CaseWhenExpression(
            new List<WhenClause>
            {
                new WhenClause(when1, then1),
                new WhenClause(when2, then2)
            },
            elseExpr
        );

        var sql = caseExpression.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("case when 1 then 'One' when 2 then 'Two' else 'Other' end", sql);
    }

    [Fact]
    public void CaseExpressionWithCase_ToSql_ReturnsCorrectSql()
    {
        var caseExpr = new ColumnExpression("TableName", "ColumnName");
        var when1 = new LiteralExpression(1);
        var then1 = ValueBuilder.Constant("'One'");
        var when2 = new LiteralExpression(2);
        var then2 = ValueBuilder.Constant("'Two'");
        var elseExpr = ValueBuilder.Constant("'Other'");

        var caseExpression = new CaseExpression(
            caseExpr,
            new List<WhenClause>
            {
                new WhenClause(when1, then1),
                new WhenClause(when2, then2)
            },
            elseExpr
        );

        var sql = caseExpression.ToSqlWithoutCte();
        output.WriteLine(sql);
        Assert.Equal("case TableName.ColumnName when 1 then 'One' when 2 then 'Two' else 'Other' end", sql);
    }

    [Fact]
    public void FunctionExpression_WithOverClause_ReturnsCorrectSql()
    {
        // Arrange
        var partitionBy = new PartitionByClause();
        partitionBy.PartitionByColumns.Add(new ColumnExpression("a", "value"));

        var orderBy = new OrderByClause();
        orderBy.OrderByColumns.Add(new OrderByColumn(new ColumnExpression("a", "id")));

        var windowFrame = new BetweenWindowFrame(
            "rows",
            new BetweenWindowFrameBoundary(
                new WindowFrameBoundaryKeyword("unbounded preceding"),
                new WindowFrameBoundaryKeyword("current row")
            )
        );

        var windowFunction = new NamelessWindowDefinition(partitionBy, orderBy, windowFrame);

        var overClause = new OverClause(windowFunction);

        var functionExpression = ValueBuilder.Function("sum", [new ColumnExpression("a", "value")], overClause);

        // Act
        var sql = functionExpression.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("sum(a.value) over(partition by a.value order by a.id rows between unbounded preceding and current row)", sql);
    }

    [Fact]
    public void FunctionExpression_WithEmptyOverClause_ReturnsCorrectSql()
    {
        // Arrange
        var overClause = new OverClause();

        var functionExpression = ValueBuilder.Function("sum", [new ColumnExpression("a", "value")], overClause);

        // Act
        var sql = functionExpression.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("sum(a.value) over()", sql);
    }
}
