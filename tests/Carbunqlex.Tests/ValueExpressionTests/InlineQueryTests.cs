using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ValueExpressionTests;

public class InlineQueryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private ColumnExpression CreateColumnExpression(string columnName)
    {
        return new ColumnExpression(columnName);
    }

    [Fact]
    public void ToSql_WithSimpleSelectQuery_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName"))
        );
        var selectQuery = new SelectQuery(selectClause);
        var inlineQuery = new InlineQuery(selectQuery);

        // Act
        var sql = inlineQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("(select ColumnName)", sql);
    }

    [Fact]
    public void ToSql_WithComplexSelectQuery_ReturnsCorrectSql()
    {
        // Arrange
        var selectClause = new SelectClause(
            new SelectExpression(CreateColumnExpression("ColumnName2"))
        );
        var fromClause = new FromClause(new DatasourceExpression(new TableSource("TableName")));
        var whereClause = new WhereClause(
            new BinaryExpression(
                "=",
                new ColumnExpression("ColumnName1"),
                new LiteralExpression(1)
                )
            );
        var selectQuery = new SelectQuery(selectClause)
        {
            FromClause = fromClause,
        };
        selectQuery.WhereClause.Add(whereClause.Condition!);
        var inlineQuery = new InlineQuery(selectQuery);

        // Act
        var sql = inlineQuery.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("(select ColumnName2 from TableName where ColumnName1 = 1)", sql);
    }
}
