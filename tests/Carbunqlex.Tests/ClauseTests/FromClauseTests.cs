using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class FromClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private static IDatasource GetDatasource()
    {
        return new TableSource("table_a", "a");
    }

    private static IValueExpression GetCondition()
    {
        return new BinaryExpression(
            "=",
            new ColumnExpression("a", "id"),
            new ConstantExpression(1)
        );
    }

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
    public void ToSql_NoJoins_ReturnsCorrectSql()
    {
        // Arrange
        var datasource = GetDatasource();
        var fromClause = new FromClause(datasource);

        // Act
        var sql = fromClause.ToSql();

        // Assert
        Assert.Equal("from table_a as a", sql);
    }

    [Fact]
    public void ToSql_WithJoins_ReturnsCorrectSql()
    {
        // Arrange
        var datasource = GetDatasource();
        var joinClause = GetInnerJoinClause();
        var fromClause = new FromClause(datasource);
        fromClause.joinClauses.Add(joinClause);

        // Act
        var sql = fromClause.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("from table_a as a inner join table_b as b on a.table_a_id = b.table_a_id and a.table_a_sub_id = b.table_a_sub_id", sql);
    }
}
