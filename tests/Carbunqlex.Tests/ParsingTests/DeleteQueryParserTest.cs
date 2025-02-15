using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class DeleteQueryParserTest
{
    private readonly ITestOutputHelper Output;

    public DeleteQueryParserTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void ParseBasicDeleteQuery()
    {
        var sql = "delete from table_name";
        var result = DeleteQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("delete from table_name", actual);
    }

    [Fact]
    public void ParseDeleteQueryWithWhere()
    {
        var sql = "delete from table_name where column1 = 1";
        var result = DeleteQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("delete from table_name where column1 = 1", actual);
    }

    [Fact]
    public void ParseDeleteQueryWithReturning()
    {
        var sql = "delete from table_name returning id";
        var result = DeleteQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("delete from table_name returning id", actual);
    }

    [Fact]
    public void ParseDeleteQueryWithWithClause()
    {
        var sql = "WITH cte(id) AS (VALUES (1), (2), (3)) DELETE FROM users AS u WHERE u.user_id IN (SELECT cte.id FROM cte)";
        var result = DeleteQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("with cte(id) as (values (1), (2), (3)) delete from users as u where u.user_id in (select cte.id from cte)", actual);
    }

    [Fact]
    public void ParseDeleteQueryWithUsingClause()
    {
        var sql = "delete from table_name using other_table where table_name.id = other_table.id";
        var result = DeleteQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("delete from table_name using other_table where table_name.id = other_table.id", actual);
    }

    [Fact]
    public void ParseInvalidDeleteQuery()
    {
        // Missing 'from' keyword
        var sql = "delete table_name";
        Assert.Throws<NotSupportedException>(() => DeleteQueryParser.Parse(sql));
    }
}
