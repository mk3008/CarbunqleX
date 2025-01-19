using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class SelectClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private ColumnExpression CreateColumnExpression(string columnName)
    {
        return new ColumnExpression(columnName);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenDistinctIsNull()
    {
        // Arrange
        var selectClause = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName")),
            new SelectExpression(CreateColumnExpression("ColumnName"), "name")
        );

        // Act
        var sql = selectClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName, ColumnName as name", sql);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenDistinctIsFalse()
    {
        // Arrange
        var selectClause = new SelectClause(
            EmptyDistinctClause.Instance,
            new SelectExpression(CreateColumnExpression("ColumnName")),
            new SelectExpression(CreateColumnExpression("ColumnName"), "name")
        );

        // Act
        var sql = selectClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName, ColumnName as name", sql);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenDistinctIsTrue()
    {
        // Arrange
        var selectClause = new SelectClause(
            new DistinctClause(),
            new SelectExpression(CreateColumnExpression("ColumnName")),
            new SelectExpression(CreateColumnExpression("ColumnName"), "name")
        );

        // Act
        var sql = selectClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select distinct ColumnName, ColumnName as name", sql);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenDistinctOnIsUsed()
    {
        // Arrange
        var distinctOnClause = new DistinctOnClause(
            CreateColumnExpression("Value1"),
            CreateColumnExpression("Value2")
        );

        var selectClause = new SelectClause(
            distinctOnClause,
            new SelectExpression(CreateColumnExpression("Value1")),
            new SelectExpression(CreateColumnExpression("Value2"), "v2")
        );

        // Act
        var sql = selectClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select distinct on (Value1, Value2) Value1, Value2 as v2", sql);
    }

    [Fact]
    public void ToSql_ShouldReturnWildcard_WhenExpressionsAreEmpty()
    {
        // Arrange
        var selectClause = new SelectClause();

        // Act
        var sql = selectClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select *", sql);
    }

    [Fact]
    public void GenerateTokens_ShouldReturnWildcard_WhenExpressionsAreEmpty()
    {
        // Arrange
        var selectClause = new SelectClause();

        // Act
        var tokens = selectClause.GenerateTokensWithoutCte();
        output.WriteLine(string.Join(", ", tokens.Select(l => l.Value)));

        // Assert
        var expected = new List<Token>
        {
            new Token(TokenType.Command, "select"),
            new Token(TokenType.Identifier, "*")
        };
        Assert.Equal(expected, tokens);
    }
}
