using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class SelectClauseTests
{
    private readonly ITestOutputHelper output;

    public SelectClauseTests(ITestOutputHelper output)
    {
        this.output = output;
    }

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
        var sql = selectClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName, ColumnName as name", sql);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenDistinctIsFalse()
    {
        // Arrange
        var selectClause = new SelectClause(
            new DistinctClause(false),
            new SelectExpression(CreateColumnExpression("ColumnName")),
            new SelectExpression(CreateColumnExpression("ColumnName"), "name")
        );

        // Act
        var sql = selectClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName, ColumnName as name", sql);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenDistinctIsTrue()
    {
        // Arrange
        var selectClause = new SelectClause(
            new DistinctClause(true),
            new SelectExpression(CreateColumnExpression("ColumnName")),
            new SelectExpression(CreateColumnExpression("ColumnName"), "name")
        );

        // Act
        var sql = selectClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select distinct ColumnName, ColumnName as name", sql);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenDistinctOnIsUsed()
    {
        // Arrange
        var distinctClause = new DistinctClause(
            CreateColumnExpression("Value1"),
            CreateColumnExpression("Value2")
        );

        var selectClause = new SelectClause(
            distinctClause,
            new SelectExpression(CreateColumnExpression("Value1")),
            new SelectExpression(CreateColumnExpression("Value2"), "v2")
        );

        // Act
        var sql = selectClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select distinct on (Value1, Value2) Value1, Value2 as v2", sql);
    }
}
