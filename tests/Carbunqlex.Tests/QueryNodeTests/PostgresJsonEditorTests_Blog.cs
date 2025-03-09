using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class PostgresJsonEditorTests_Blog(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private string QueryCommandText = """
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

    [Fact]
    public void PostArrayJson()
    {
        var query = SelectQueryParser.Parse(QueryCommandText);
        var queryNode = QueryAstParser.Parse(query);
        queryNode.Where("blog_id", w => w.Equal(":blog_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.ArraySerialize("posts", objectName: "posts", upperNode: static x =>
                {
                    return x.Serialize("blogs", objectName: "blog", upperNode: static x =>
                    {
                        return x.Serialize("organizations", objectName: "organization");
                    }).Serialize("users", objectName: "user");
                });
            });

        var actual = queryNode.Query.ToSql();

        output.WriteLine(actual);

        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where blogs.blog_id = :blog_id), __json_posts as (select json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'blog', json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)), 'user', json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name))) as posts from __json) select row_to_json(d) from (select __json_posts.posts as \"posts\" from __json_posts) as d limit 1";
        Assert.Equal(expected, actual);

        /* return sample
{
  "posts": [
    {
      "post_id": 9,
      "title": "Understanding AI",
      "content": "This is a post about AI.",
      "created_at": "2025-02-18T20:25:21.974106",
      "blog": {
        "blog_id": 1,
        "blog_name": "AI Insights",
        "organization": {"organization_id": 1, "organization_name": "Tech Corp"}
      },
      "user": {"user_id": 1, "user_name": "Alice"}
    },
    {
      "post_id": 11,
      "title": "Understanding AI",
      "content": "This is a post about AI.",
      "created_at": "2025-03-07T17:53:49.168646",
      "blog": {
        "blog_id": 1,
        "blog_name": "AI Insights",
        "organization": {"organization_id": 1, "organization_name": "Tech Corp"}
      },
      "user": {"user_id": 1, "user_name": "Alice"}
    }
  ]
}
         */
    }

    [Fact]
    public void PostJson()
    {
        var query = SelectQueryParser.Parse(QueryCommandText);
        var queryNode = QueryAstParser.Parse(query);
        queryNode.Where("post_id", w => w.Equal(":post_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.Serialize("posts", objectName: "post", upperNode: static x =>
                {
                    return x.Serialize("blogs", objectName: "blog", upperNode: static x =>
                    {
                        return x.Serialize("organizations", objectName: "organization");
                    }).Serialize("users", objectName: "user");
                });
            });

        var actual = queryNode.Query.ToSql();

        output.WriteLine(actual);

        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where posts.post_id = :post_id) select row_to_json(d) from (select json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'blog', json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)), 'user', json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name)) as \"post\" from __json) as d limit 1";
        Assert.Equal(expected, actual);

        /* return sample
{
  "post": {
    "post_id": 9,
    "title": "Understanding AI",
    "content": "This is a post about AI.",
    "created_at": "2025-02-18T20:25:21.974106",
    "blog": {
      "blog_id": 1,
      "blog_name": "AI Insights",
      "organization": {
        "organization_id": 1,
        "organization_name": "Tech Corp"
      }
    },
    "user": {
      "user_id": 1,
      "user_name": "Alice"
    }
  }
}
         */
    }

    [Fact]
    public void UserJson()
    {
        var query = SelectQueryParser.Parse(QueryCommandText);

        var queryNode = QueryAstParser.Parse(query);
        queryNode.Where("user_id", w => w.Equal(":user_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.Serialize("users", objectName: "user", upperNode: static x =>
                {
                    return x.ArraySerialize("posts", objectName: "posts", upperNode: static x =>
                    {
                        return x.Serialize("blogs", objectName: "blog", upperNode: static x =>
                        {
                            return x.Serialize("organizations", objectName: "organization");
                        });
                    });
                });
            });

        var actual = queryNode.Query.ToSql();

        output.WriteLine(actual);

        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where users.user_id = :user_id), __json_posts as (select __json.users__user_id, __json.users__user_name, json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'blog', json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)))) as users__posts from __json group by __json.users__user_id, __json.users__user_name) select row_to_json(d) from (select json_build_object('user_id', __json_posts.users__user_id, 'user_name', __json_posts.users__user_name, 'posts', __json_posts.users__posts) as \"user\" from __json_posts) as d limit 1";
        Assert.Equal(expected, actual);

        /* return sample
{
  "user": {
    "user_id": 1,
    "user_name": "Alice",
    "posts": [
      {
        "post_id": 9,
        "title": "Understanding AI",
        "content": "This is a post about AI.",
        "created_at": "2025-02-18T20:25:21.974106",
        "blog": {
          "blog_id": 1,
          "blog_name": "AI Insights",
          "organization": {
            "organization_id": 1,
            "organization_name": "Tech Corp"
          }
        }
      },
      {
        "post_id": 11,
        "title": "Understanding AI",
        "content": "This is a post about AI.",
        "created_at": "2025-03-07T17:53:49.168646",
        "blog": {
          "blog_id": 1,
          "blog_name": "AI Insights",
          "organization": {
            "organization_id": 1,
            "organization_name": "Tech Corp"
          }
        }
      }
    ]
  }
}
     */
    }

    [Fact]
    public void UserArrayJson()
    {
        var query = SelectQueryParser.Parse(QueryCommandText);

        var queryNode = QueryAstParser.Parse(query);
        queryNode = queryNode.ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.ArraySerialize("users", objectName: "users", upperNode: static x =>
                {
                    return x.ArraySerialize("posts", objectName: "posts", upperNode: static x =>
                    {
                        return x.Serialize("blogs", objectName: "blog", upperNode: static x =>
                        {
                            return x.Serialize("organizations", objectName: "organization");
                        });
                    });
                });
            });

        var actual = queryNode.Query.ToSql();
        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id), __json_posts as (select __json.users__user_id, __json.users__user_name, json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'blog', json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)))) as users__posts from __json group by __json.users__user_id, __json.users__user_name), __json_users as (select json_agg(json_build_object('user_id', __json_posts.users__user_id, 'user_name', __json_posts.users__user_name, 'posts', __json_posts.users__posts)) as users from __json_posts) select row_to_json(d) from (select __json_users.users as \"users\" from __json_users) as d limit 1";

        output.WriteLine($"/*expected*/ {expected}");
        output.WriteLine($"/*actual  */ {actual}");

        Assert.Equal(expected, actual);

        /* return sample
{
  "users": [
    {
      "user_id": 2,
      "user_name": "Bob",
      "posts": [
        {
          "post_id": 10,
          "title": "Latest Design Trends",
          "content": "Exploring modern design.",
          "created_at": "2025-02-18T20:25:21.974106",
          "blog": {
            "blog_id": 2,
            "blog_name": "Design Trends",
            "organization": {
              "organization_id": 2,
              "organization_name": "Creative Hub"
            }
          }
        }
      ]
    },
    {
      "user_id": 1,
      "user_name": "Alice",
      "posts": [
        {
          "post_id": 9,
          "title": "Understanding AI",
          "content": "This is a post about AI.",
          "created_at": "2025-02-18T20:25:21.974106",
          "blog": {
            "blog_id": 1,
            "blog_name": "AI Insights",
            "organization": {
              "organization_id": 1,
              "organization_name": "Tech Corp"
            }
          }
        }
      ]
    }
  ]
}
         */
    }

    [Fact]
    public void OrganizationJson()
    {
        var query = SelectQueryParser.Parse(QueryCommandText);
        var queryNode = QueryAstParser.Parse(query);
        queryNode.Where("organization_id", w => w.Equal(":organization_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.Serialize("organizations", objectName: "organization", upperNode: static x =>
                {
                    return x.ArraySerialize("blogs", objectName: "blogs", upperNode: static x =>
                    {
                        return x.ArraySerialize("posts", objectName: "posts", upperNode: static x =>
                        {
                            return x.Serialize("users", objectName: "user");
                        });
                    });
                });
            });
        var actual = queryNode.Query.ToSql();

        output.WriteLine(actual);
        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where organizations.organization_id = :organization_id), __json_posts as (select __json.blogs__blog_id, __json.blogs__blog_name, __json.organizations__organization_id, __json.organizations__organization_name, json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'user', json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name))) as blogs__posts from __json group by __json.blogs__blog_id, __json.blogs__blog_name, __json.organizations__organization_id, __json.organizations__organization_name), __json_blogs as (select __json_posts.organizations__organization_id, __json_posts.organizations__organization_name, json_agg(json_build_object('blog_id', __json_posts.blogs__blog_id, 'blog_name', __json_posts.blogs__blog_name, 'posts', __json_posts.blogs__posts)) as organizations__blogs from __json_posts group by __json_posts.organizations__organization_id, __json_posts.organizations__organization_name) select row_to_json(d) from (select json_build_object('organization_id', __json_blogs.organizations__organization_id, 'organization_name', __json_blogs.organizations__organization_name, 'blogs', __json_blogs.organizations__blogs) as \"organization\" from __json_blogs) as d limit 1";
        Assert.Equal(expected, actual);

        /* return sample
{
  "organization": {
    "organization_id": 1,
    "organization_name": "Tech Corp",
    "blogs": [
      {
        "blog_id": 1,
        "blog_name": "AI Insights",
        "posts": [
          {
            "post_id": 9,
            "title": "Understanding AI",
            "content": "This is a post about AI.",
            "created_at": "2025-02-18T20:25:21.974106",
            "user": {
              "user_id": 1,
              "user_name": "Alice"
            }
          },
          {
            "post_id": 11,
            "title": "Understanding AI",
            "content": "This is a post about AI.",
            "created_at": "2025-03-07T17:53:49.168646",
            "user": {
              "user_id": 1,
              "user_name": "Alice"
            }
          }
        ]
      }
    ]
  }
}
        */
    }

    [Fact]
    public void OrganizationArrayJson()
    {
        var query = SelectQueryParser.Parse(QueryCommandText);
        var queryNode = QueryAstParser.Parse(query);
        queryNode.Where("post_id", w => w.Equal(":post_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.ArraySerialize("organizations", objectName: "organizations", upperNode: static x =>
                {
                    return x.ArraySerialize("blogs", objectName: "blogs", upperNode: static x =>
                    {
                        return x.ArraySerialize("posts", objectName: "posts", upperNode: static x =>
                        {
                            return x.Serialize("users", objectName: "user");
                        });
                    });
                });
            });
        var actual = queryNode.Query.ToSql();

        output.WriteLine(actual);
        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where posts.post_id = :post_id), __json_posts as (select __json.blogs__blog_id, __json.blogs__blog_name, __json.organizations__organization_id, __json.organizations__organization_name, json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'user', json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name))) as blogs__posts from __json group by __json.blogs__blog_id, __json.blogs__blog_name, __json.organizations__organization_id, __json.organizations__organization_name), __json_blogs as (select __json_posts.organizations__organization_id, __json_posts.organizations__organization_name, json_agg(json_build_object('blog_id', __json_posts.blogs__blog_id, 'blog_name', __json_posts.blogs__blog_name, 'posts', __json_posts.blogs__posts)) as organizations__blogs from __json_posts group by __json_posts.organizations__organization_id, __json_posts.organizations__organization_name), __json_organizations as (select json_agg(json_build_object('organization_id', __json_blogs.organizations__organization_id, 'organization_name', __json_blogs.organizations__organization_name, 'blogs', __json_blogs.organizations__blogs)) as organizations from __json_blogs) select row_to_json(d) from (select __json_organizations.organizations as \"organizations\" from __json_organizations) as d limit 1";
        Assert.Equal(expected, actual);

        /* return sample
{
  "organizations": [
    {
      "organization_id": 1,
      "organization_name": "Tech Corp",
      "blogs": [
        {
          "blog_id": 1,
          "blog_name": "AI Insights",
          "posts": [
            {
              "post_id": 9,
              "title": "Understanding AI",
              "content": "This is a post about AI.",
              "created_at": "2025-02-18T20:25:21.974106",
              "user": {
                "user_id": 1,
                "user_name": "Alice"
              }
            }
          ]
        }
      ]
    }
  ]
}
        */
    }
}
