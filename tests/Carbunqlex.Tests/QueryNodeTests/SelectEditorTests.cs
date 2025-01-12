using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class SelectEditorTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void GreatestAndLeastTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Select("value", static value => value.Greatest(1).Least(10));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, least(greatest(a.value, 1), 10) as value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CoalesceTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Select("value", static value => value.Coalesce(1, 2, 3));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, coalesce(a.value, 1, 2, 3) as value from table_a as a";
        Assert.Equal(expected, actual);
    }

    //[Fact]
    //public void AddColumnTest()
    //{
    //    // Arrange
    //    var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

    //    // Act
    //    var queryNode = QueryNodeFactory.Create(query);
    //    output.WriteLine(queryNode.Query.ToSql());

    //    queryNode.When("value", r =>
    //    {
    //        r.AddColumn(r.Value, "value2").Coalesce(0);
    //    });

    //    query.AddColumn(ValueBuilder.Keyword("current_timestamp"), "created_at");
    //    query.AddColumn(ValueBuilder.Keyword("current_timestamp"), "updated_at");

    //    var actual = queryNode.Query.ToSql();
    //    output.WriteLine(actual);

    //    var expected = "select a.table_a_id, a.value, coalesce(a.value, 0) as value2, current_timestamp as created_at, current_timestamp as updated_at from table_a as a";
    //    Assert.Equal(expected, actual);
    //}

    [Fact]
    public void RemoveColumnTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Remove("value");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id from table_a as a";
        Assert.Equal(expected, actual);
    }
}
