using Carbunqlex.Clauses;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class ForClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Theory]
    [InlineData("update", "for update")]
    [InlineData("share", "for share")]
    [InlineData("no key update", "for no key update")]
    [InlineData("key share", "for key share")]
    public void ToSql_ReturnsExpectedSqlString(string lockType, string expectedSql)
    {
        // Arrange
        var forClause = new ForClause(lockType);

        // Act
        var sql = forClause.ToSqlWithoutCte();
        output.WriteLine(sql);

        // Assert
        Assert.Equal(expectedSql, sql);
    }
}
