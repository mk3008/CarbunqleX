using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class UpdateQueryParserTest
{
    private readonly ITestOutputHelper Output;

    public UpdateQueryParserTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void ParseBasicUpdateQuery()
    {
        var sql = "update table_name set column1 = 1, column2 = 'value' where column3 = 2";
        var result = UpdateQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("update table_name set column1 = 1, column2 = 'value' where column3 = 2", actual);
    }

    [Fact]
    public void ParseUpdateQueryWithReturning()
    {
        var sql = "update table_name set column1 = 1, column2 = 'value' where column3 = 2 returning id";
        var result = UpdateQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("update table_name set column1 = 1, column2 = 'value' where column3 = 2 returning id", actual);
    }

    [Fact]
    public void ParseFrom()
    {
        var sql = "UPDATE posts p SET title = u.new_title FROM updates u WHERE p.post_id = u.post_id";
        var result = UpdateQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("update posts as p set title = u.new_title from updates as u where p.post_id = u.post_id", actual);
    }

    [Fact]
    public void ParseCte()
    {
        var sql = "WITH updated_data AS (SELECT post_id, new_title FROM updates)UPDATE posts p SET title = u.new_title FROM updated_data u WHERE p.post_id = u.post_id;";
        var result = UpdateQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("with updated_data as (select post_id, new_title from updates) update posts as p set title = u.new_title from updated_data as u where p.post_id = u.post_id", actual);
    }
}
