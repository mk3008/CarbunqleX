using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class CreateTableAsQueryParserTest
{
    private readonly ITestOutputHelper Output;

    public CreateTableAsQueryParserTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void ParseBasicCreateTableAsQuery()
    {
        var sql = "create table table_name as select column1, column2 from other_table";
        var result = CreateTableAsQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("create table table_name as select column1, column2 from other_table", actual);
    }

    [Fact]
    public void ParseTemporaryCreateTableAsQuery()
    {
        var sql = "create temporary table table_name as select column1, column2 from other_table";
        var result = CreateTableAsQueryParser.Parse(sql);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("create temporary table table_name as select column1, column2 from other_table", actual);
    }
}
