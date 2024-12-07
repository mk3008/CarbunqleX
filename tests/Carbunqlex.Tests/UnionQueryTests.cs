using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Carbunqlex.Tests;

public class UnionQueryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private ColumnExpression CreateColumnExpression(string columnName)
    {
        return new ColumnExpression(columnName);
    }

    [Fact]
    public void ToSql_WithUnion_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause1 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName1"))
        );
        var selectQuery1 = new SelectQuery(selectClause1);

        var selectClause2 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName2"))
        );
        var selectQuery2 = new SelectQuery(selectClause2);

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.Union);

        // Act
        var sql = unionQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 union select ColumnName2", sql);
    }

    [Fact]
    public void ToSql_WithUnionAll_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause1 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName1"))
        );
        var selectQuery1 = new SelectQuery(selectClause1);

        var selectClause2 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName2"))
        );
        var selectQuery2 = new SelectQuery(selectClause2);

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.UnionAll);

        // Act
        var sql = unionQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 union all select ColumnName2", sql);
    }

    [Fact]
    public void ToSql_WithIntersect_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause1 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName1"))
        );
        var selectQuery1 = new SelectQuery(selectClause1);

        var selectClause2 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName2"))
        );
        var selectQuery2 = new SelectQuery(selectClause2);

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.Intersect);

        // Act
        var sql = unionQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 intersect select ColumnName2", sql);
    }

    [Fact]
    public void ToSql_WithExcept_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause1 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName1"))
        );
        var selectQuery1 = new SelectQuery(selectClause1);

        var selectClause2 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName2"))
        );
        var selectQuery2 = new SelectQuery(selectClause2);

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.Except);

        // Act
        var sql = unionQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 except select ColumnName2", sql);
    }

    [Fact]
    public void ToSql_WithThreeQueries_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause1 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName1"))
        );
        var selectQuery1 = new SelectQuery(selectClause1);

        var selectClause2 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName2"))
        );
        var selectQuery2 = new SelectQuery(selectClause2);

        var selectClause3 = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName3"))
        );
        var selectQuery3 = new SelectQuery(selectClause3);

        var unionQuery1 = new UnionQuery(selectQuery1, selectQuery2, UnionType.Union);
        var unionQuery2 = new UnionQuery(unionQuery1, selectQuery3, UnionType.UnionAll);

        // Act
        var sql = unionQuery2.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 union select ColumnName2 union all select ColumnName3", sql);
    }
}
