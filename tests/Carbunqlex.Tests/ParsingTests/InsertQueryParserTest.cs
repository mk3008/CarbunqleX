using Carbunqlex.Parsing;
using Carbunqlex.Parsing.ValueExpression;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class InsertQueryParserTest
{
    private readonly ITestOutputHelper Output;

    public InsertQueryParserTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void ParseBasicInsertQuery()
    {
        var sql = "insert into table_name(column1, column2) values (1, 'value')";
        var result = InsertQueryParser.Parse(sql);
        var actual = result.ToSqlWithoutCte();
        Output.WriteLine(actual);
        Assert.Equal("insert into table_name(column1, column2) values (1, 'value')", actual);
    }

    [Fact]
    public void ParseInsertQueryWithReturning()
    {
        var sql = "insert into table_name(column1, column2) values (1, 'value') returning id";
        var result = InsertQueryParser.Parse(sql);
        var actual = result.ToSqlWithoutCte();
        Output.WriteLine(actual);
        Assert.Equal("insert into table_name(column1, column2) values (1, 'value') returning id", actual);
    }

    [Fact]
    public void ParseInsertQueryWithSelect()
    {
        var sql = "insert into table_name(column1, column2) select column1, column2 from other_table";
        var result = InsertQueryParser.Parse(sql);
        var actual = result.ToSqlWithoutCte();
        Output.WriteLine(actual);
        Assert.Equal("insert into table_name(column1, column2) select column1, column2 from other_table", actual);
    }

    [Fact]
    public void ParseInvalidInsertQuery()
    {
        // Missing '(' after table_name
        var sql = "insert into table_name column1, column2 values (1, 'value')";
        Assert.Throws<SqlParsingException>(() => InsertQueryParser.Parse(sql));
    }
}
