using Carbunqlex.Clauses;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class WithClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_WithoutRecursiveCommonTableClauses_ReturnsCorrectSql()
    {
        // Arrange
        var commonTableClause1 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table1"), "cte1");
        var commonTableClause2 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table2"), "cte2");

        var withClause = new WithClause(commonTableClause1, commonTableClause2);

        // Act
        var sql = withClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with cte1 as (select * from table1), cte2 as (select * from table2)", sql);
    }

    [Fact]
    public void ToSql_WithRecursiveCommonTableClauses_ReturnsCorrectSql()
    {
        // Arrange
        var commonTableClause1 = new CommonTableClause(SelectQueryFactory.CreateSelectRecursiveQuery(), "number_series", isRecursive: true);
        var commonTableClause2 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table2"), "cte2");

        var withClause = new WithClause(commonTableClause1, commonTableClause2);

        // Act
        var sql = withClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with recursive number_series as (select 1 as number union all select number + 1 from number_series where number < 10), cte2 as (select * from table2)", sql);
    }

    [Fact]
    public void ToSql_WithMixedRecursiveCommonTableClauses_RecursiveFirst()
    {
        // Arrange
        var commonTableClause1 = new CommonTableClause(SelectQueryFactory.CreateSelectRecursiveQuery(), "number_series", isRecursive: true);
        var commonTableClause2 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table2"), "cte2");

        var withClause = new WithClause(commonTableClause2, commonTableClause1);

        // Act
        var sql = withClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("with recursive number_series as (select 1 as number union all select number + 1 from number_series where number < 10), cte2 as (select * from table2)", sql);
    }

    [Fact]
    public void TryValidate_DuplicateCteNamesWithDifferentSql_ReturnsError()
    {
        // Arrange
        var cte1 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table1"), "cte1");
        var cte2 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table2"), "cte1");
        var withClause = new WithClause(cte1, cte2);

        // Act
        var isValid = withClause.TryValidate(out var errorMessages);
        foreach (var errorMessage in errorMessages)
        {
            output.WriteLine(errorMessage);
        }

        // Assert
        Assert.False(isValid);
        Assert.Contains("Duplicate CTE name 'cte1' found at indices: 0, 1 with different SQL definitions.", errorMessages);
    }

    [Fact]
    public void TryValidate_DuplicateCteNamesWithSameSql_ReturnsNoError()
    {
        // Arrange
        var cte1 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table1"), "cte1");
        var cte2 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table1"), "cte1");
        var withClause = new WithClause(cte1, cte2);

        // Act
        var isValid = withClause.TryValidate(out var errorMessages);
        foreach (var errorMessage in errorMessages)
        {
            output.WriteLine(errorMessage);
        }

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMessages);
    }

    [Fact]
    public void TryValidate_MultipleRecursiveCtes_ReturnsError()
    {
        // Arrange
        var cte1 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table1"), "cte1", isRecursive: true);
        var cte2 = new CommonTableClause(SelectQueryFactory.CreateSelectAllQuery("table2"), "cte2", isRecursive: true);
        var withClause = new WithClause(cte1, cte2);

        // Act
        var isValid = withClause.TryValidate(out var errorMessages);
        foreach (var errorMessage in errorMessages)
        {
            output.WriteLine(errorMessage);
        }

        // Assert
        Assert.False(isValid);
        Assert.Contains("Multiple recursive CTEs found.", errorMessages);
    }
}
