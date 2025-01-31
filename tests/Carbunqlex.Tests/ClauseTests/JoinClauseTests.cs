using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class JoinClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private static JoinClause GetInnerJoinClause()
    {
        var source = new DatasourceExpression(new TableSource("table_b"), "b");
        var condition = new BinaryExpression(
            "and"
            , new BinaryExpression(
                "=",
                new ColumnExpression("a", "table_a_id"),
                new ColumnExpression("b", "table_a_id")
                )
            , new BinaryExpression(
                "=",
                new ColumnExpression("a", "table_a_sub_id"),
                new ColumnExpression("b", "table_a_sub_id")
                )
        );
        return new JoinClause(source, "inner join", condition);
    }

    private static JoinClause GetCrossJoinClause()
    {
        var source = new DatasourceExpression(new TableSource("table_b"), "b");
        return new JoinClause(source, "cross join");
    }

    [Fact]
    public void ToSql_WithCondition_ReturnsCorrectSql()
    {
        // Arrange
        var joinClause = GetInnerJoinClause();

        // Act
        var sql = joinClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("inner join table_b as b on a.table_a_id = b.table_a_id and a.table_a_sub_id = b.table_a_sub_id", sql);
    }

    [Fact]
    public void ToSql_WithoutCondition_ReturnsCorrectSql()
    {
        // Arrange
        var joinClause = GetCrossJoinClause();

        // Act
        var sql = joinClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("cross join table_b as b", sql);
    }
}
