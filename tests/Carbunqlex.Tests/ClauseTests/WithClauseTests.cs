using Carbunqlex.Clauses;
using Xunit.Abstractions;

namespace Carbunqlex.Tests;

public class WithClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ToSql_ShouldReturnEmptyString_WhenNoCommonTableClauses()
    {
        // Arrange
        var withClause = new WithClause();

        // Act
        var sql = withClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal(string.Empty, sql);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenCommonTableClausesProvided()
    {
        // Arrange
        var commonTableClause1 = new CommonTableClause(new MockQuery("SELECT * FROM table1"), "cte1");
        var commonTableClause2 = new CommonTableClause(new MockQuery("SELECT * FROM table2"), "cte2");
        var withClause = new WithClause(false, commonTableClause1, commonTableClause2);

        // Act
        var sql = withClause.ToSql();
        output.WriteLine(sql);

        // Assert
        var expectedSql = "with cte1 as (SELECT * FROM table1), cte2 as (SELECT * FROM table2)";
        Assert.Equal(expectedSql, sql);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql_WhenIsRecursiveIsTrue()
    {
        // Arrange
        var commonTableClause = new CommonTableClause(new MockQuery("SELECT * FROM table"), "cte");
        var withClause = new WithClause(true, commonTableClause);

        // Act
        var sql = withClause.ToSql();
        output.WriteLine(sql);

        // Assert
        var expectedSql = "with recursive cte as (SELECT * FROM table)";
        Assert.Equal(expectedSql, sql);
    }

    // Simple implementation of IQuery for testing purposes
    private class MockQuery : IQuery
    {
        private readonly string _sql;

        public MockQuery(string sql)
        {
            _sql = sql;
        }

        public string ToSql()
        {
            return _sql;
        }

        public IEnumerable<Lexeme> GetLexemes()
        {
            return Enumerable.Empty<Lexeme>();
        }

        public IEnumerable<Lexeme> GenerateLexemes()
        {
            throw new NotImplementedException();
        }

        public string ToSqlWithoutCte()
        {
            return _sql;
        }

        public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommonTableClause> GetCommonTableClauses()
        {
            throw new NotImplementedException();
        }
    }
}
