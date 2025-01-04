using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryTests;

public class WhereModifierTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void EqualAndNotEqualTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier
                .Equal(100)
                .NotEqual(-100);
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id = 100 and a.table_a_id <> -100";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GreaterThanAndGreaterThanOrEqualTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier
                .GreaterThan(50)
                .GreaterThanOrEqual(30);
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id > 50 and a.table_a_id >= 30";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LessThanAndLessThanOrEqualTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier
                .LessThan(200)
                .LessThanOrEqual(300);
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id < 200 and a.table_a_id <= 300";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InAndNotInTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier
                .In(1, 2, 3)
                .NotIn(4, 5, 6);
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id in (1, 2, 3) and a.table_a_id not in (4, 5, 6)";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LikeAndNotLikeTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);


        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier
                .Like("%a%")
                .NotLike("%b%");
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id like '%a%' and a.table_a_id not like '%b%'";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AnyTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier
                .Any(1, 2, 3)
                .Any(r.AddParameter(":prm", new int[] { 1, 2, 3 }));
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id = any(array[1, 2, 3]) and a.table_a_id = any(:prm)";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsNullTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier
                .IsNull()
                .IsNotNull();
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id is null and a.table_a_id is not null";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BetweenTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier
                .Between(1, 10)
                .NotBetween(20, 30);
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where a.table_a_id between 1 and 10 and a.table_a_id not between 20 and 30";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ValueModifiedTest()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.When("table_a_id", r =>
        {
            r.WhereModifier.Coalesce(0)
                .GreaterThanOrEqual(0);
        });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a where coalesce(a.table_a_id, 0) >= 0";
        Assert.Equal(expected, actual);
    }
}
