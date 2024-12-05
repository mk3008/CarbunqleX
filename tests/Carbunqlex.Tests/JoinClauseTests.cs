using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.QueryModels;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.Clauses;

public class JoinClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private static JoinClause GetInnerJoinClause()
    {
        var source = new TableSource("table_b", "b");
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
        return new JoinClause(source, JoinType.Inner, condition);
    }

    private static JoinClause GetCrossJoinClause()
    {
        var source = new TableSource("table_b", "b");
        return new JoinClause(source, JoinType.Cross);
    }

    [Fact]
    public void ToSql_WithCondition_ReturnsCorrectSql()
    {
        // Arrange
        var joinClause = GetInnerJoinClause();

        // Act
        var sql = joinClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal(" inner join table_b as b on a.table_a_id = b.table_a_id and a.table_a_sub_id = b.table_a_sub_id", sql);
    }

    [Fact]
    public void ToSql_WithoutCondition_ReturnsCorrectSql()
    {
        // Arrange
        var joinClause = GetCrossJoinClause();

        // Act
        var sql = joinClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal(" cross join table_b as b", sql);
    }
}
