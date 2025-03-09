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


    private string QueryText = """
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
        """;

    /// <summary>
    /// Sample to generate a query that returns JSON
    /// </summary>
    [Fact]
    public void ToJsonQuery()
    {
        var query = QueryAstParser.Parse(QueryText);

        query.Where("post_id", action: x => x.Equal(":post_id"))
            .NormalizeSelectClause()
            .ToJsonQuery(x =>
            {
                return x.Serialize(datasource: "posts", jsonKey: "post", parent: static x =>
                {
                    return x.Serialize(datasource: "users", jsonKey: "user")
                        .Serialize(datasource: "blogs", jsonKey: "blog", parent: static x =>
                        {
                            return x.Serialize(datasource: "organizations", jsonKey: "organization");
                        });
                });
            });

        var actual = query.ToSql();

        output.WriteLine(actual);

        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where posts.post_id = :post_id) select row_to_json(d) from (select json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'user', json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name), 'blog', json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name))) as \"post\" from __json) as d limit 1";
        Assert.Equal(expected, actual);

        /* JSON Sample 
{
  "post": {
    "post_id": 9,
    "title": "Understanding AI",
    "content": "This is a post about AI.",
    "created_at": "2025-02-18T20:25:21.974106",
    "user": {
      "user_id": 1,
      "user_name": "Alice"
    },
    "blog": {
      "blog_id": 1,
      "blog_name": "AI Insights",
      "organization": {
        "organization_id": 1,
        "organization_name": "Tech Corp"
      }
    }
  }
}
         */
    }

    [Fact]
    public void ToJsonQueryFlat()
    {
        var query = QueryAstParser.Parse(QueryText);

        query.Where("post_id", action: x => x.Equal(":post_id"))
            .NormalizeSelectClause()
            .ToJsonQuery(x =>
            {
                return x.Serialize("posts", isFlat: true, parent: static x =>
                {
                    return x.Serialize("users", jsonKey: "user")
                        .Serialize("blogs", jsonKey: "blog", parent: static x =>
                        {
                            return x.Serialize("organizations", jsonKey: "organization");
                        });
                });
            });

        var actual = query.ToSql();

        output.WriteLine(actual);

        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where posts.post_id = :post_id) select row_to_json(d) from (select __json.posts__post_id as \"post_id\", __json.posts__title as \"title\", __json.posts__content as \"content\", __json.posts__created_at as \"created_at\", json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name) as \"user\", json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)) as \"blog\" from __json) as d limit 1";
        Assert.Equal(expected, actual);

        /* JSON Sample 
{
  "post_id": 9,
  "title": "Understanding AI",
  "content": "This is a post about AI.",
  "created_at": "2025-02-18T20:25:21.974106",
  "user": {
    "user_id": 1,
    "user_name": "Alice"
  },
  "blog": {
    "blog_id": 1,
    "blog_name": "AI Insights",
    "organization": {
      "organization_id": 1,
      "organization_name": "Tech Corp"
    }
  }
}
         */
    }

}
