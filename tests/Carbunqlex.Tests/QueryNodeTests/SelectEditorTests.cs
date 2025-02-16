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

    [Fact]
    public void ToDeleteQueryTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.v1 as value_1, a.value_2 from table_a as a where a.price = 1");
        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());
        var deleteQuery = queryNode.ToDeleteQuery("table_x");
        var actual = deleteQuery.ToSqlWithoutCte();
        output.WriteLine(actual);
        var expected = "delete from table_x where (table_x.value_1, table_x.value_2) in (select q.value_1, q.value_2 from (select a.v1 as value_1, a.value_2 from table_a as a where a.price = 1) as q)";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToDeleteQueryWithReturningTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.v1 as value_1, a.value_2 from table_a as a where a.price = 1");
        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());
        var updateQuery = queryNode.ToDeleteQuery("table_x", hasReturning: true);
        var actual = updateQuery.ToSqlWithoutCte();
        output.WriteLine(actual);
        var expected = "delete from table_x where (table_x.value_1, table_x.value_2) in (select q.value_1, q.value_2 from (select a.v1 as value_1, a.value_2 from table_a as a where a.price = 1) as q) returning *";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToDeleteQueryWithKeyColumnTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a where a.table_a_id = 1");
        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());
        var deleteQuery = queryNode.ToDeleteQuery("table_b", keyColumns: ["table_a_id"]);
        var actual = deleteQuery.ToSqlWithoutCte();
        output.WriteLine(actual);
        var expected = "delete from table_b where table_b.table_a_id in (select q.table_a_id from (select a.table_a_id, a.value from table_a as a where a.table_a_id = 1) as q)";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToCreateTableQueryTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");
        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());
        var createTableAsQuery = queryNode.ToCreateTableQuery("table_b", false);
        var actual = createTableAsQuery.ToSql();
        output.WriteLine(actual);
        var expected = "create table table_b as select a.table_a_id, a.value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToCreateTemporaryTableQueryTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");
        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.Query.ToSql());
        var createTableAsQuery = queryNode.ToCreateTableQuery("table_b", true);
        var actual = createTableAsQuery.ToSql();
        output.WriteLine(actual);
        var expected = "create temporary table table_b as select a.table_a_id, a.value from table_a as a";
        Assert.Equal(expected, actual);
    }
}
