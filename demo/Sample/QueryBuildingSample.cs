using Carbunqlex;
using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.QuerySources;
using Xunit.Abstractions;

namespace Sample;

/// <summary>
/// Sample for query building.
/// </summary>
/// <param name="output"></param>
public class QueryBuildingSample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    /// <summary>
    /// Sample for writing SelectQuery without using RawSQL.
    /// </summary>
    [Fact]
    public void BuildSelectQuery()
    {
        var fromClause = new FromClause(new DatasourceExpression(new TableSource("table_a"), "a"));
        var selectExpressions = new List<SelectExpression>()
        {
            new SelectExpression(new ColumnExpression("a.table_a_id")),
            new SelectExpression(new LiteralExpression(1), "value")
        };
        var selectClause = new SelectClause(selectExpressions);

        var sq = new SelectQuery(selectClause, fromClause);

        var expected = "select a.table_a_id, 1 as value from table_a as a";

        var actual = sq.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);

        // If RawSQL is available, it is recommended to write it like this.
        var query = QueryAstParser.Parse(expected);
        Assert.Equal(expected, query.ToSql());
    }

    /// <summary>
    /// Sample for writing SelectQuery with CTE.
    /// </summary>
    [Fact]
    public void BuildSelectQueryWithCte()
    {
        var fromClause = new FromClause(new DatasourceExpression(new TableSource("cte"), "x"));
        var selectExpressions = new List<SelectExpression>()
        {
            new SelectExpression(new ColumnExpression("*"))
        };
        var selectClause = new SelectClause(selectExpressions);
        var sq = new SelectQuery(selectClause, fromClause);

        // create with clause
        var subQuery = QueryAstParser.Parse("select a.table_a_id, 1 as value from table_a as a");
        sq.WithClause.Add(new CommonTableClause(subQuery, "cte"));

        var expected = "with cte as (select a.table_a_id, 1 as value from table_a as a) select * from cte as x";
        var actual = sq.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
        // If RawSQL is available, it is recommended to write it like this.
        var query = QueryAstParser.Parse(expected);
        Assert.Equal(expected, query.ToSql());
    }

    /// <summary>
    /// Sample for generating a create table query from a select query.
    /// </summary>
    [Fact]
    public void CreateTableQuerySample()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, 1 as value from table_a as a");

        var createTableQuery = query.ToCreateTableQuery("table_b", isTemporary: true);

        var expected = "create temporary table table_b as select a.table_a_id, 1 as value from table_a as a";

        var actual = createTableQuery.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Sample for generating an insert table query from a select query.
    /// </summary>
    [Fact]
    public void InsertQuerySample()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, 1 as value from table_a as a");

        var insertTableQuery = query.ToInsertQuery("table_b", hasReturning: true);

        var expected = "insert into table_b(table_a_id, value) select a.table_a_id, 1 as value from table_a as a returning *";

        var actual = insertTableQuery.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Sample for generating an update query from a select query.
    /// </summary>
    [Fact]
    public void UpdateQuerySample()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, 1 as value from table_a as a");

        var updateQuery = query.ToUpdateQuery("table_b", ["table_a_id"]);

        var expected = "update table_b set value = q.value from (select a.table_a_id, 1 as value from table_a as a) as q where table_b.table_a_id = q.table_a_id";

        var actual = updateQuery.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Sample for generating a delete query from a select query.
    /// </summary>
    [Fact]
    public void DeleteQuerySample()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, 1 as value from table_a as a");

        var deleteQuery = query.ToDeleteQuery("table_b", ["table_a_id"]);

        var expected = "delete from table_b where table_b.table_a_id in (select q.table_a_id from (select a.table_a_id, 1 as value from table_a as a) as q)";

        var actual = deleteQuery.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Sample for generating a union query.
    /// </summary>
    [Fact]
    public void UnionAll()
    {
        var query1 = QueryAstParser.Parse("select id from table_a");
        var query2 = QueryAstParser.Parse("select id from table_b");
        var unionQuery = query1.UnionAll(query2);

        var expected = "select id from table_a union all select id from table_b";

        var actual = unionQuery.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Sample for generating a distinct union query.
    /// Combine multiple queries to generate a query that removes duplicates.
    /// </summary>
    [Fact]
    public void UnionAllAndDistinct()
    {
        var query1 = QueryAstParser.Parse("select id from table_a");
        var query2 = QueryAstParser.Parse("select id from table_b");
        var distinctQuery = query1.UnionAll(query2).ToSubQuery().Distinct();

        var expected = "select distinct * from (select id from table_a union all select id from table_b) as d";

        var actual = distinctQuery.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }
}
