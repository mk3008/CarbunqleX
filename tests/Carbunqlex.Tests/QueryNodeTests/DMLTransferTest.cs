using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;
public class DMLTransferTest(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;


    [Fact]
    public void ToInsertQueryMultipleSequence()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        var insertQuery = root.ToInsertQuery("table_b", sequenceColumns: ["sequence_column1", "sequence_column2"], hasReturning: true);

        var actual = insertQuery.ToSql();
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
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        var insertQuery = root.ToInsertQuery("table_b");

        var actual = insertQuery.ToSql();
        output.WriteLine(actual);

        var expected = "insert into table_b(table_a_id, value) select a.table_a_id, a.value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInsertQueryWithCteTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("WITH cte AS (SELECT post_id FROM posts) SELECT post_id FROM cte");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var insertQuery = root.ToInsertQuery("table_b");
        var actual = insertQuery.ToSql();
        output.WriteLine(actual);
        var expected = "insert into table_b(post_id) with cte as (select post_id from posts) select post_id from cte";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInsertQueryWithSingleSequenceColumnTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        var insertQuery = root.ToInsertQuery("table_b", sequenceColumns: ["sequence_column1"], hasReturning: true);

        var actual = insertQuery.ToSql();
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
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        var insertQuery = root.ToInsertQuery("table_b", sequenceColumns: ["sequence_column1"], hasReturning: true);

        var actual = insertQuery.ToSql();
        output.WriteLine(actual);

        var expected = "insert into table_b(id, val) select a.table_a_id as id, a.value as val from table_a as a returning sequence_column1, id, val";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToInsertQueryWithValueFilter()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value1, a.value2 from table_a as a where a.table_a_id = 1");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        var insertQuery = root.ToInsertQuery("table_b", valueColumns: ["value2"]);

        var actual = insertQuery.ToSql();
        output.WriteLine(actual);

        var expected = "insert into table_b(value2) select q.value2 from (select a.table_a_id, a.value1, a.value2 from table_a as a where a.table_a_id = 1) as q";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToUpdateQueryTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a where a.price = 1");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        var updateQuery = root.ToUpdateQuery("table_b", ["table_a_id"]);
        var actual = updateQuery.ToSql();
        output.WriteLine(actual);

        var expected = "update table_b set value = q.value from (select a.table_a_id, a.value from table_a as a where a.price = 1) as q where table_b.table_a_id = q.table_a_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToUpdateQueryWithReturningTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a where a.price = 1");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var updateQuery = root.ToUpdateQuery("table_b", ["table_a_id"], hasReturning: true);
        var actual = updateQuery.ToSql();
        output.WriteLine(actual);
        var expected = "update table_b set value = q.value from (select a.table_a_id, a.value from table_a as a where a.price = 1) as q where table_b.table_a_id = q.table_a_id returning *";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToUpdateQueryWithCteTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("WITH cte AS (SELECT post_id, value FROM posts) SELECT post_id, value FROM cte");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var updateQuery = root.ToUpdateQuery("table_b", ["post_id"]);
        var actual = updateQuery.ToSql();
        output.WriteLine(actual);
        var expected = "with cte as (select post_id, value from posts) update table_b set value = q.value from (select post_id, value from cte) as q where table_b.post_id = q.post_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToUpdateQueryWithKeyColumnTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a where a.table_a_id = 1");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var updateQuery = root.ToUpdateQuery("table_b", ["table_a_id"]);
        var actual = updateQuery.ToSql();
        output.WriteLine(actual);
        var expected = "update table_b set value = q.value from (select a.table_a_id, a.value from table_a as a where a.table_a_id = 1) as q where table_b.table_a_id = q.table_a_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToUpdateQueryWithValueFilter()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value1, a.value2 from table_a as a where a.table_a_id = 1");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var updateQuery = root.ToUpdateQuery("table_b", ["table_a_id"], valueColumns: ["value2"]);
        var actual = updateQuery.ToSql();
        output.WriteLine(actual);
        var expected = "update table_b set value2 = q.value2 from (select a.table_a_id, a.value1, a.value2 from table_a as a where a.table_a_id = 1) as q where table_b.table_a_id = q.table_a_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToDeleteQueryWithReturningTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.v1 as value_1, a.value_2 from table_a as a where a.price = 1");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var deleteQuery = root.ToDeleteQuery("table_x", hasReturning: true);
        var actual = deleteQuery.ToSql();
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
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var deleteQuery = root.ToDeleteQuery("table_b", keyColumns: ["table_a_id"]);
        var actual = deleteQuery.ToSql();
        output.WriteLine(actual);
        var expected = "delete from table_b where table_b.table_a_id in (select q.table_a_id from (select a.table_a_id, a.value from table_a as a where a.table_a_id = 1) as q)";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToDeleteQueryCte()
    {
        // Arrange
        var query = SelectQueryParser.Parse("WITH cte AS (SELECT post_id FROM posts) SELECT post_id FROM cte");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var deleteQuery = root.ToDeleteQuery("table_b");
        var actual = deleteQuery.ToSql();
        output.WriteLine(actual);
        var expected = "with cte as (select post_id from posts) delete from table_b where table_b.post_id in (select q.post_id from (select post_id from cte) as q)";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToCreateTableQueryTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var createTableAsQuery = root.ToCreateTableQuery("table_b", false);
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
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var createTableAsQuery = root.ToCreateTableQuery("table_b", true);
        var actual = createTableAsQuery.ToSql();
        output.WriteLine(actual);
        var expected = "create temporary table table_b as select a.table_a_id, a.value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToCreateTableQueryWithCteTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("WITH cte AS (SELECT post_id FROM posts) SELECT post_id FROM cte");
        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());
        var createTableAsQuery = root.ToCreateTableQuery("table_b", false);
        var actual = createTableAsQuery.ToSql();
        output.WriteLine(actual);
        var expected = "create table table_b as with cte as (select post_id from posts) select post_id from cte";
        Assert.Equal(expected, actual);
    }
}
