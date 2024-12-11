using Carbunqlex.Clauses;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ClauseTests;

public class ForClauseTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Theory]
    [InlineData(LockType.Update, "for update")]
    [InlineData(LockType.Share, "for share")]
    [InlineData(LockType.NoKeyUpdate, "for no key update")]
    [InlineData(LockType.KeyShare, "for key share")]
    public void ToSql_ReturnsExpectedSqlString(LockType lockType, string expectedSql)
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
