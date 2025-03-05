using Carbunqlex;
using Xunit.Abstractions;

namespace Sample;

/// <summary>
/// Sample for Postgres specific features.
/// </summary>
/// <param name="output"></param>
public class PostgresSpecificSample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    /// <summary>
    /// Sample to generate a query that returns JSON
    /// </summary>
    [Fact]
    public void ToJsonQuery()
    {
        var query = QueryAstParser.Parse("""  
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
          """);

        query.NormalizeSelectClause()
            .Serialize("organizations", objectName: "organization")
            .Serialize("users", objectName: "user")
            .Serialize("blogs", objectName: "blog", include: ["organization"])
            .Serialize("posts", objectName: "post", include: ["user", "blog"])
            .ToJson();

        var actual = query.ToSql();

        output.WriteLine(actual);

        var expected = "select row_to_json(d) from (select json_build_object('post_id', posts.post_id, 'title', posts.title, 'content', posts.content, 'created_at', posts.created_at, 'user', json_build_object('user_id', users.user_id, 'user_name', users.name), 'blog', json_build_object('blog_id', blogs.blog_id, 'blog_name', blogs.name, 'organization', json_build_object('organization_id', organizations.organization_id, 'organization_name', organizations.name))) as post from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where posts.post_id = :post_id) as d limit 1";
        Assert.Equal(expected, actual);
    }
}
