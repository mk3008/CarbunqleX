using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class GroupByEditorTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void AddGroupCondition()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select u.user_id, u.name from user as u");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.GroupBy("user_id");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select u.user_id, u.name from user as u group by u.user_id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddGroupCondition_Alias()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select u.id as user_id, u.name from user as u");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.GroupBy("user_id");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select u.id as user_id, u.name from user as u group by u.id";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddGroupCondition_Terminal()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select u2.user_id, u2.name from (select u1.user_id, u1.name from user as u1) as u2");

        // Act
        var queryNode = QueryAstParser.Create(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.GroupBy("user_id");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select u2.user_id, u2.name from (select u1.user_id, u1.name from user as u1) as u2 group by u2.user_id";
        Assert.Equal(expected, actual);
    }
}
