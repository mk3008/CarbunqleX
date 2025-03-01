using Carbunqlex.Expressions;
using Carbunqlex.QuerySources;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class FromEditorTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void JoinTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");
        var queryNode = QueryAstParser.Parse(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.From(["table_a_id"], isCurrentOnly: true, static from =>
        {
            from.Join("inner join"
                , new DatasourceExpression(new TableSource("table_b"), "b")
                , static (map, ds) =>
                {
                    return map.Values
                        .Select(x => x.Equal(new ColumnExpression(ds.Alias, x.DefaultName)))
                        .Aggregate((current, next) => current.And(next));
                });
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
        var queryNode = QueryAstParser.Parse(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.From(["table_a_id"], isCurrentOnly: true, static from =>
        {
            from.InnerJoin("table_b", "b");
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
        var queryNode = QueryAstParser.Parse(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.From(["table_a_id", "value"], isCurrentOnly: true, static from =>
        {
            from.InnerJoin("table_b", "b");
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
        var queryNode = QueryAstParser.Parse(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.From(["table_a_id"], isCurrentOnly: true, static from =>
        {
            from.LeftJoin("table_b", "b").EditQuery(static q =>
            {
                q.AddColumn("coalesce(b.amount, 0)", "quantity");
                q.AddColumn("b.name");
            });
        });

        output.WriteLine(queryNode.ToTreeString());

        var actual = queryNode.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value, coalesce(b.amount, 0) as quantity, b.name from table_a as a left join table_b as b on a.table_a_id = b.table_a_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void JoinAndFilterTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");
        var queryNode = QueryAstParser.Parse(query);

        // Act
        output.WriteLine(queryNode.ToSql());

        queryNode.From(["table_a_id"], isCurrentOnly: true, static from =>
        {
            from.LeftJoin("table_b", "b")
                .EditQuery(static q =>
                {
                    q.Where("coalesce(b.amount, 0) = 0");
                    q.Where("b.name ='test'");
                });
        });

        output.WriteLine(queryNode.ToTreeString());

        var actual = queryNode.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a left join table_b as b on a.table_a_id = b.table_a_id where coalesce(b.amount, 0) = 0 and b.name = 'test'";
        Assert.Equal(expected, actual);
    }
}
