using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class SelectEditorTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void GreatestAndLeastTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Override("value", static value => value.Greatest(1).Least(10));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, least(greatest(a.value, 1), 10) as value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CoalesceTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Override("value", static value => value.Coalesce(1, 2, 3));

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, coalesce(a.value, 1, 2, 3) as value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RemoveColumnTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Remove("value");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInsertQueryMultipleSequence()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        var insertQuery = queryNode.ToInsertQuery("table_b", true, "sequence_column1", "sequence_column2");

        var actual = insertQuery.ToSqlWithoutCte();
        output.WriteLine(actual);

        var expected = "insert into table_b(table_a_id, value) select a.table_a_id, a.value from table_a as a returning sequence_column1, sequence_column2, table_a_id, value";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInsertQueryWithoutReturningTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        var insertQuery = queryNode.ToInsertQuery("table_b");

        var actual = insertQuery.ToSqlWithoutCte();
        output.WriteLine(actual);

        var expected = "insert into table_b(table_a_id, value) select a.table_a_id, a.value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInsertQueryWithSingleSequenceColumnTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        var insertQuery = queryNode.ToInsertQuery("table_b", true, "sequence_column1");

        var actual = insertQuery.ToSqlWithoutCte();
        output.WriteLine(actual);

        var expected = "insert into table_b(table_a_id, value) select a.table_a_id, a.value from table_a as a returning sequence_column1, table_a_id, value";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInsertQueryWithColumnAliasTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id as id, a.value as val from table_a as a");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        var insertQuery = queryNode.ToInsertQuery("table_b", true, "sequence_column1");

        var actual = insertQuery.ToSqlWithoutCte();
        output.WriteLine(actual);

        var expected = "insert into table_b(id, val) select a.table_a_id as id, a.value as val from table_a as a returning sequence_column1, id, val";
        Assert.Equal(expected, actual);
    }
}
