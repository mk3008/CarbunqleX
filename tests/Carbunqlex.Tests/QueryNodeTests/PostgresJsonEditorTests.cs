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


        var newQueryNode = queryNode.ToSubQuery("d");

        var actual = newQueryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select d.user_id, d.name from (select users.user_id, users.name from users) as d";
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

        var expected = "select users.user_id as users_user_id, users.name as users_name from users";
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

        var expected = "select u.user_id as u_id, u.name as u_name from users as u";
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

        queryNode.AddJsonColumn("u", "user");

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

        queryNode.AddJsonColumn("users", "user");

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

        queryNode.AddJsonColumn("users", "user");

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

        queryNode.AddJsonColumn("users", "user", propertyBuilder: upperCaseBuilder);

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select json_build_object('USER_ID', users.user_id, 'NAME', users.name) as user from users";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddJsonColumnStartWith()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id, users.name as user_name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Serialize("user", "user");

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
        var query = SelectQueryParser.Parse("select users.user_id, users.name as user_name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.Serialize("user", "user", propertyBuilder: upperCaseBuilder);

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

        queryNode.ToJson();

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select row_to_json(d) from (select users.user_id as id, users.name from users) as d limit 1";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToJsonArray()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select users.user_id as id, users.name from users");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.Query.ToSql());

        queryNode.ToJsonArray();

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select json_agg(row_to_json(d)) from (select users.user_id as id, users.name from users) as d";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Test()
    {
        var sql = """
            select
                posts.post_id
                , posts.title
                , posts.content
                , posts.created_at
                , users.user_id
                , users.name as user_name
                , blogs.blog_id
                , blogs.name as blog_name
                , organizations.organization_id
                , organizations.name as organization_name
            from
                posts
                inner join users on posts.user_id = users.user_id
                inner join blogs on posts.blog_id = blogs.blog_id
                inner join organizations on blogs.organization_id = organizations.organization_id
            where
                posts.post_id = :post_id
            """;

        var query = SelectQueryParser.Parse(sql);
        var queryNode = QueryAstParser.Parse(query);
        queryNode.NormalizeSelectClause()
            .Serialize("organizations", objectName: "organization")
            .Serialize("users", objectName: "user")
            .Serialize("blogs", objectName: "blog", include: ["organization"])
            .Serialize("posts", objectName: "post", include: ["user", "blog"])
            .ToJson();

        var actual = queryNode.Query.ToSql();

        output.WriteLine(actual);

        var expected = "select row_to_json(d) from (select json_build_object('post_id', posts.post_id, 'title', posts.title, 'content', posts.content, 'created_at', posts.created_at, 'user', json_build_object('user_id', users.user_id, 'user_name', users.name), 'blog', json_build_object('blog_id', blogs.blog_id, 'blog_name', blogs.name, 'organization', json_build_object('organization_id', organizations.organization_id, 'organization_name', organizations.name))) as post from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where posts.post_id = :post_id) as d limit 1";
        Assert.Equal(expected, actual);
    }
}
