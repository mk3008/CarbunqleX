using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class WhereEditorTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void EqualAndNotEqualTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("table_a_id", static value => value.Equal(100).NotEqual(-100));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id = 100 and a.table_a_id <> -100";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GreaterThanAndGreaterThanOrEqualTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("table_a_id", static value => value.GreaterThan(50).GreaterThanOrEqual(30));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id > 50 and a.table_a_id >= 30";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LessThanAndLessThanOrEqualTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("table_a_id", static value => value.LessThan(200).LessThanOrEqual(300));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id < 200 and a.table_a_id <= 300";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InAndNotInTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("table_a_id", static value => value.In([1, 2, 3]).NotIn([4, 5, 6]));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id in (1, 2, 3) and a.table_a_id not in (4, 5, 6)";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LikeAndNotLikeTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);

        queryNode.Where("table_a_id", static r => r.Like("'%a%'").NotLike("'%b%'"));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id like '%a%' and a.table_a_id not like '%b%'";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AnyTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("table_a_id",
            static r => r.Any(1, 2, 3).Any(r.AddParameter(":prm", new int[] { 1, 2, 3 })));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id = any(array[1, 2, 3]) and a.table_a_id = any(:prm)";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsNullTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("table_a_id", static value => value.IsNull().IsNotNull());

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id is null and a.table_a_id is not null";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BetweenTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("table_a_id", static value => value.Between(1, 10).NotBetween(20, 30));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id between 1 and 10 and a.table_a_id not between 20 and 30";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ValueModifiedTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("table_a_id", static value => value.Coalesce(0, w => w.GreaterThanOrEqual(0)));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where coalesce(a.table_a_id, 0) >= 0";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SubQueryTest()
    {
        var query = SelectQueryParser.Parse("select d.user_id, d.users_name from (select u.user_id, u.users_name from users as u) as d");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("user_id", static value => value.Equal(10));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select d.user_id, d.users_name from (select u.user_id, u.users_name from users as u where u.user_id = 10) as d";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CommonQueryTest()
    {
        var query = SelectQueryParser.Parse("with d as (select u.user_id, u.users_name from users as u) select d.user_id, d.users_name from d");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("user_id", static value => value.Equal(10));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "with d as (select u.user_id, u.users_name from users as u where u.user_id = 10) select d.user_id, d.users_name from d";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void UnionQueryTest()
    {
        var query = SelectQueryParser.Parse("""
            select u.user_id, u.users_name from users as u
            union 
            select u.user_id, u.users_name from users as u
            union all
            select u.user_id, u.users_name from users as u
            """);

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("user_id", static value => value.Equal(10));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);
        output.WriteLine(queryNode.ToTreeString());

        var expected = "select u.user_id, u.users_name from users as u where u.user_id = 10 union select u.user_id, u.users_name from users as u where u.user_id = 10 union all select u.user_id, u.users_name from users as u where u.user_id = 10";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ComplexQueryTest()
    {
        var query = SelectQueryParser.Parse("""
            with cte as (select u.user_id, u.users_name from users as u)
            -- standard
            select u.user_id, u.users_name from users as u
            union 
            -- subquery
            select d.user_id, d.users_name from (select u.user_id, u.users_name from users as u) as d
            union all
            -- subquery and cte
            select d.user_id, d.users_name from (select u.user_id, u.users_name from cte as u) as d
            union all
            -- cte
            select c1.user_id, c1.users_name from cte as c1
            union all
            -- cte
            select c2.user_id, c2.users_name from cte as c2
            union all
            -- subquery and union
            select d.user_id, d.users_name from (select u1.user_id, u1.users_name from users as u1 union all select u2.user_id, u2.users_name from users as u2) as d
            """);

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Where("user_id", static value => value.Equal(10));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "with cte as (select u.user_id, u.users_name from users as u where u.user_id = 10) select u.user_id, u.users_name from users as u where u.user_id = 10 union select d.user_id, d.users_name from (select u.user_id, u.users_name from users as u where u.user_id = 10) as d union all select d.user_id, d.users_name from (select u.user_id, u.users_name from cte as u) as d union all select c1.user_id, c1.users_name from cte as c1 union all select c2.user_id, c2.users_name from cte as c2 union all select d.user_id, d.users_name from (select u1.user_id, u1.users_name from users as u1 where u1.user_id = 10 union all select u2.user_id, u2.users_name from users as u2 where u2.user_id = 10) as d";
        Assert.Equal(expected, actual);
    }
}
