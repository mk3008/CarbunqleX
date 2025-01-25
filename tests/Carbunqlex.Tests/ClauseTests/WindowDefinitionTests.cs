using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class WindowDefinitionTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_WithAllComponents_ReturnsCorrectSql()
    {
        // Arrange
        var partitionBy = new PartitionByClause();
        partitionBy.PartitionByColumns.Add(new ColumnExpression("a", "value"));

        var orderBy = new OrderByClause();
        orderBy.OrderByColumns.Add(new OrderByColumn(new ColumnExpression("a", "id")));

        var windowFrame = new WindowFrame("rows",
            new BetweenWindowFrameBoundary(
                new WindowFrameBoundaryKeyword("unbounded preceding"),
                new WindowFrameBoundaryKeyword("current row")
                )
            );

        var windowFunction = new NamelessWindowDefinition(partitionBy, orderBy, windowFrame);

        // Act
        var result = windowFunction.ToSqlWithoutCte();
        output.WriteLine(result);

        // Assert
        Assert.Equal("(partition by a.value order by a.id rows between unbounded preceding and current row)", result);
    }
}
