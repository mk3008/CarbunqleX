using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class ValueQueryParserTests
{
    public ValueQueryParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Fact]
    public void ParseValuesQuery()
    {
        var sql = """
            values (1, 2), (3, 4), (5, 6)
            """;

        // Arrange
        var tokenizer = new SqlTokenizer(sql);
        // Act
        var result = SelectQueryParser.Parse(tokenizer);
        var actual = result.ToSql();
        Output.WriteLine(actual);

        Assert.Equal("values (1, 2), (3, 4), (5, 6)", actual);
    }

    [Fact]
    public void ParseValuesUnionQuery()
    {
        var sql = """
            values (1, 2), (3, 4), (5, 6)
            union
            values (7, 8), (9, 10), (11, 12)
            """;
        // Arrange
        var tokenizer = new SqlTokenizer(sql);
        // Act
        var result = SelectQueryParser.Parse(tokenizer);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("values (1, 2), (3, 4), (5, 6) union values (7, 8), (9, 10), (11, 12)", actual);
    }

    [Fact]
    public void ParseValuesQueryWithSubquery()
    {
        var sql = """
            values (1, (SELECT 'A')), (2, (SELECT 'B'))
            """;
        // Arrange
        var tokenizer = new SqlTokenizer(sql);
        // Act
        var result = SelectQueryParser.Parse(tokenizer);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("values (1, (select 'A')), (2, (select 'B'))", actual);
    }

    [Fact]
    public void ParseValuesQueryWithCteInSubquery()
    {
        // Although not syntactically correct in SQL, Carbunqlex allows CTEs within subqueries.
        // However, when written back to SQL, the CTE will be output in its proper position.
        var sql = """
            VALUES
            (1, (
                WITH max_users AS (SELECT MAX(user_id) AS max_id FROM users)
                SELECT max_id FROM max_users
                )
            ),
            (2, 3)
            """;
        // Arrange
        var tokenizer = new SqlTokenizer(sql);
        // Act
        var result = SelectQueryParser.Parse(tokenizer);
        var actual = result.ToSql();
        Output.WriteLine(actual);
        Assert.Equal("with max_users as (select MAX(user_id) as max_id from users) values (1, (select max_id from max_users)), (2, 3)", actual);
    }
}
