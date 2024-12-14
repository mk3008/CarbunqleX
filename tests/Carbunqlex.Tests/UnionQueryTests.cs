using Xunit.Abstractions;

namespace Carbunqlex.Tests;

public class UnionQueryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_WithUnion_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectQuery("ColumnName1");
        var selectQuery2 = SelectQueryFactory.CreateSelectQuery("ColumnName2");

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.Union);

        // Act
        var sql = unionQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 union select ColumnName2", sql);
    }

    [Fact]
    public void ToSql_WithUnionAll_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectQuery("ColumnName1");
        var selectQuery2 = SelectQueryFactory.CreateSelectQuery("ColumnName2");

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.UnionAll);

        // Act
        var sql = unionQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 union all select ColumnName2", sql);
    }

    [Fact]
    public void ToSql_WithIntersect_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectQuery("ColumnName1");
        var selectQuery2 = SelectQueryFactory.CreateSelectQuery("ColumnName2");

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.Intersect);

        // Act
        var sql = unionQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 intersect select ColumnName2", sql);
    }

    [Fact]
    public void ToSql_WithExcept_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectQuery("ColumnName1");
        var selectQuery2 = SelectQueryFactory.CreateSelectQuery("ColumnName2");

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.Except);

        // Act
        var sql = unionQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select ColumnName1 except select ColumnName2", sql);
    }

    [Fact]
    public void ToSql_WithThreeWithClauses_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectQueryWithWithClause("cte1");
        var selectQuery2 = SelectQueryFactory.CreateSelectQueryWithWithClause("cte2");
        var selectQuery3 = SelectQueryFactory.CreateSelectQueryWithWithClause("cte3");

        var unionQuery1 = new UnionQuery(selectQuery1, selectQuery2, UnionType.Union);
        var unionQuery2 = new UnionQuery(unionQuery1, selectQuery3, UnionType.UnionAll);

        // Act
        var sql = unionQuery2.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte1 as (SELECT * FROM table), cte2 as (SELECT * FROM table), cte3 as (SELECT * FROM table) select ColumnName1 from cte1 union select ColumnName1 from cte2 union all select ColumnName1 from cte3", sql);
    }

    [Fact]
    public void ToSql_WithWithClauseUnion_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectQueryWithWithClause("cte");
        var selectQuery2 = SelectQueryFactory.CreateSelectQueryWithWithClause("cte");

        var unionQuery = new UnionQuery(selectQuery1, selectQuery2, UnionType.Union);

        // Act
        var sql = unionQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte as (SELECT * FROM table) select ColumnName1 from cte union select ColumnName1 from cte", sql);
    }
}
