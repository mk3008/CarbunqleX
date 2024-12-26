using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryTests;

public class UnionQueryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_WithUnion_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectConstantQuery(1, "value1");
        var selectQuery2 = SelectQueryFactory.CreateSelectConstantQuery(2, "value2");

        var unionQuery = new UnionQuery(UnionType.Union, selectQuery1, selectQuery2);

        // Act
        var sql = unionQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select 1 as value1 union select 2 as value2", sql);
    }

    [Fact]
    public void ToSql_WithUnionAll_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectConstantQuery(1, "value1");
        var selectQuery2 = SelectQueryFactory.CreateSelectConstantQuery(2, "value2");

        var unionQuery = new UnionQuery(UnionType.UnionAll, selectQuery1, selectQuery2);

        // Act
        var sql = unionQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select 1 as value1 union all select 2 as value2", sql);
    }

    [Fact]
    public void ToSql_WithIntersect_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectConstantQuery(1, "value1");
        var selectQuery2 = SelectQueryFactory.CreateSelectConstantQuery(2, "value2");

        var unionQuery = new UnionQuery(UnionType.Intersect, selectQuery1, selectQuery2);

        // Act
        var sql = unionQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select 1 as value1 intersect select 2 as value2", sql);
    }

    [Fact]
    public void ToSql_WithExcept_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectConstantQuery(1, "value1");
        var selectQuery2 = SelectQueryFactory.CreateSelectConstantQuery(2, "value2");

        var unionQuery = new UnionQuery(UnionType.Except, selectQuery1, selectQuery2);

        // Act
        var sql = unionQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("select 1 as value1 except select 2 as value2", sql);
    }

    [Fact]
    public void ToSql_WithThreeWithClauses_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table1", "cte1");
        var selectQuery2 = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table2", "cte2");
        var selectQuery3 = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table3", "cte3");

        var unionQuery1 = new UnionQuery(UnionType.Union, selectQuery1, selectQuery2);
        var unionQuery2 = new UnionQuery(UnionType.UnionAll, unionQuery1, selectQuery3);

        // Act
        var sql = unionQuery2.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte1 as (select * from table1), cte2 as (select * from table2), cte3 as (select * from table3) select * from cte1 union select * from cte2 union all select * from cte3", sql);
    }

    [Fact]
    public void ToSql_WithWithClauseUnion_ReturnsCorrectSql()
    {
        // Arrange
        var selectQuery1 = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table1", "cte");
        var selectQuery2 = SelectQueryFactory.CreateSelectAllQueryWithWithClause("table1", "cte");

        var unionQuery = new UnionQuery(UnionType.Union, selectQuery1, selectQuery2);

        // Act
        var sql = unionQuery.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte as (select * from table1) select * from cte union select * from cte", sql);
    }
}
