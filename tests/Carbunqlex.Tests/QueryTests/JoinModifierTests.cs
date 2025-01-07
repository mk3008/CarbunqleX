using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryTests;

public class JoinModifierTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void JoinTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");
        var queryNode = QueryNodeFactory.Create(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.JoinModifier(["table_a_id"], r =>
        {
            r.Join(JoinType.Inner, new TableSource("table_b", "b"), r.Values["table_a_id"].Equal(new ColumnExpression("b", "table_a_id")));
        });

        var actual = queryNode.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a inner join table_b as b on a.table_a_id = b.table_a_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InnerJoinTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");
        var queryNode = QueryNodeFactory.Create(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.JoinModifier(["table_a_id"], r =>
        {
            r.InnerJoin("table_b", "b");
        });

        var actual = queryNode.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a inner join table_b as b on a.table_a_id = b.table_a_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InnerJoinTest_CompositeKey()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");
        var queryNode = QueryNodeFactory.Create(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.JoinModifier(["table_a_id", "value"], r =>
        {
            r.InnerJoin("table_b", "b");
        });

        var actual = queryNode.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a inner join table_b as b on a.table_a_id = b.table_a_id and a.value = b.value";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void JoinAndSelectTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");
        var queryNode = QueryNodeFactory.Create(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.JoinModifier(["table_a_id"], r =>
        {
            var modifier = r.InnerJoin("table_b", "b");
            modifier.AddColumn("amount", "quantity").Coalesce(0);
            modifier.AddColumn("name");
        });

        output.WriteLine(queryNode.ToTreeString());

        var actual = queryNode.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value, coalesce(b.amount, 0) as quantity, b.name from table_a as a inner join table_b as b on a.table_a_id = b.table_a_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void JoinAndFilterTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");
        var queryNode = QueryNodeFactory.Create(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.JoinModifier(["table_a_id"], r =>
        {
            var modifier = r.InnerJoin("table_b", "b");
            modifier.Filter("amount").Equal(10);
            modifier.Filter("name").Equal("test");
        });

        output.WriteLine(queryNode.ToTreeString());

        var actual = queryNode.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a inner join table_b as b on a.table_a_id = b.table_a_id where b.amount = 10 and b.name = 'test'";
        Assert.Equal(expected, actual);
    }

    // Multiple columns
}
