using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class PostgresJsonEditorTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void TestToSubQuery()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id, users.name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        var actual = queryNode.ToSubQuery("d").ToSql();
        output.WriteLine(actual);

        var expected = "select d.user_id, d.name from (select users.user_id, users.name from users) as d";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestToCteQuery()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id, users.name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        var actual = queryNode.ToCteQuery("d").ToSql();
        output.WriteLine(actual);

        var expected = "with d as (select users.user_id, users.name from users) select d.user_id, d.name from d";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestAddDatasourceNameToColumns()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id, users.name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.NormalizeSelectClause();

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select users.user_id as users__user_id, users.name as users__name from users";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestAddDatasourceNameToColumns_Alias()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select u.user_id as id, u.name from users as u");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.NormalizeSelectClause();

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select u.user_id as u__id, u.name as u__name from users as u";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddJsonColumn_Alias()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select u.user_id, u.name from users as u");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        var editor = new PostgresJsonEditor(queryNode);
        editor.AddJsonColumn("u", "user");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select json_build_object('user_id', u.user_id, 'name', u.name) as user from users as u";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddJsonColumn()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id, users.name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        var editor = new PostgresJsonEditor(queryNode);
        editor.AddJsonColumn("users", "user");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select json_build_object('user_id', users.user_id, 'name', users.name) as user from users";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddJsonColumn_ColumnAlias()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id as id, users.name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        var editor = new PostgresJsonEditor(queryNode);
        editor.AddJsonColumn("users", "user");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select json_build_object('id', users.user_id, 'name', users.name) as user from users";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddJsonColumn_PropertyBuilder()
    {
        var upperCaseBuilder = (string s) => s.ToUpper();

        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id, users.name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        var editor = new PostgresJsonEditor(queryNode, propertyBuilder: upperCaseBuilder);
        editor.AddJsonColumn("users", "user");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select json_build_object('USER_ID', users.user_id, 'NAME', users.name) as user from users";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddJsonColumnStartWith()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id as user__id, users.name as user__name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        var editor = new PostgresJsonEditor(queryNode);
        editor.Serialize("user", "user");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select json_build_object('id', users.user_id, 'name', users.name) as user from users";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddJsonColumnStartWith_PropertyBuilder()
    {
        var upperCaseBuilder = (string s) => s.ToUpper();

        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id as user__id, users.name as user__name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        var editor = new PostgresJsonEditor(queryNode, propertyBuilder: upperCaseBuilder);
        editor.Serialize("user", "user");

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select json_build_object('ID', users.user_id, 'NAME', users.name) as user from users";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToJson()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id as id, users.name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.ToJsonQuery();

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "with __json as (select users.user_id as id, users.name from users) select row_to_json(d) from (select __json.id as \"id\", __json.name as \"name\" from __json) as d limit 1";
        Assert.Equal(expected, actual);
    }
}
