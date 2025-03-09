using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class PostgresJsonEditorTests_Blog(ITestOutputHelper output)
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

    private string JsonTestQuery = """
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
    public void TestPostJsonSerialization()
    {
        var query = SelectQueryParser.Parse(JsonTestQuery);
        var queryNode = QueryAstParser.Parse(query);
        queryNode.Where("post_id", w => w.Equal(":post_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.ArraySerialize("posts", objectName: "post", upperNode: static x =>
                {
                    return x.Serialize("blogs", objectName: "blog", upperNode: static x =>
                    {
                        return x.Serialize("organizations", objectName: "organization");
                    }).Serialize("users", objectName: "user");
                });
            });

        var actual = queryNode.Query.ToSql();

        output.WriteLine(actual);

        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where posts.post_id = :post_id), __json_post as (select json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'blog', json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)), 'user', json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name))) as post from __json) select row_to_json(d) from (select __json_post.post as \"post\" from __json_post) as d limit 1";
        Assert.Equal(expected, actual);

        /* return sample
{
    "post": {
        "post_id": 1,
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
    public void TestUserJson()
    {
        var query = SelectQueryParser.Parse(JsonTestQuery);

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

        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where users.user_id = :user_id) select row_to_json(d) from (select __json.users__user_id as \"user_id\", __json.users__user_name as \"user_name\", json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'blog', json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)))) as \"posts\" from __json group by __json.users__user_id, __json.users__user_name) as d limit 1";
        Assert.Equal(expected, actual);

        /* return sample
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
     */
    }


    [Fact]
    public void TestOrganizationJsonSerialization()
    {
        var query = SelectQueryParser.Parse(JsonTestQuery);
        var queryNode = QueryAstParser.Parse(query);
        queryNode.Where("organization_id", w => w.Equal(":organization_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.ArraySerialize("users", objectName: "users", upperNode: static x =>
                {
                    return x.Serialize("posts", objectName: "posts")
                        .Serialize("blogs", objectName: "blogs", upperNode: static x =>
                        {
                            return x.Serialize("organizations", objectName: "organization");
                        });
                });
            });
        var actual = queryNode.Query.ToSql();

        output.WriteLine(actual);
        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where organizations.organization_id = :organization_id), __json_posts as (select __json.blogs__blog_id, __json.blogs__blog_name, json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name) as blogs__organization, json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'user', json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name))) as blogs__posts from __json group by __json.blogs__blog_id, __json.blogs__blog_name, __json.organizations__organization_id, __json.organizations__organization_name), __json_blogs as (select json_agg(json_build_object('blog_id', __json_posts.blogs__blog_id, 'blog_name', __json_posts.blogs__blog_name, 'organization', __json_posts.blogs__organization, 'posts', __json_posts.blogs__posts)) as blogs from __json_posts) select row_to_json(d) from (select __json_blogs.blogs as \"blogs\" from __json_blogs) as d limit 1";
        Assert.Equal(expected, actual);

        /*
{
  "blogs": [
    {
      "blog_id": 1,
      "blog_name": "AI Insights",
      "organization": {
        "organization_id": 1,
        "organization_name": "Tech Corp"
      },
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
        */
    }

    [Fact]
    public void TestOrganizationJsonSerialization_2()
    {
        var query = SelectQueryParser.Parse(JsonTestQuery);

        var queryNode = QueryAstParser.Parse(query);
        queryNode = queryNode.Where("organization_id", w => w.Equal(":organization_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.ArraySerialize("users", objectName: "users", upperNode: static x =>
                {
                    return x.ArraySerialize("blogs", objectName: "blogs", upperNode: static x =>
                    {
                        return x.Serialize("organizations", objectName: "organization")
                            .ArraySerialize("posts", objectName: "posts", upperNode: static x =>
                            {
                                return x.Serialize("users", objectName: "user");
                            });
                    });
                });
                //    .ArraySerialize("posts", objectName: "posts", include: ["user"])
                //    .ArraySerialize("blogs", objectName: "blogs", include: ["posts"]);
                //.Serialize("organizations", objectName: "organization", include: ["blogs"]);
            });
        var actual = queryNode.Query.ToSql();
        var expected = "with __json as (select posts.post_id as posts_post_id, posts.title as posts_title, posts.content as posts_content, posts.created_at as posts_created_at, users.user_id as users_user_id, users.name as users_user_name, blogs.blog_id as blogs_blog_id, blogs.name as blogs_blog_name, organizations.organization_id as organizations_organization_id, organizations.name as organizations_organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where organizations.organization_id = :organization_id), __json_post as (select __json.blogs_blog_id, __json.blogs_blog_name, __json.organizations_organization_id, __json.organizations_organization_name, json_agg(json_build_object('post_id', __json.posts_post_id, 'title', __json.posts_title, 'content', __json.posts_content, 'created_at', __json.posts_created_at, 'user', json_build_object('user_id', __json.users_user_id, 'user_name', __json.users_user_name))) as posts from __json group by __json.blogs_blog_id, __json.blogs_blog_name, __json.organizations_organization_id, __json.organizations_organization_name) select row_to_json(d) from (select __json_post.blogs_blog_id, __json_post.blogs_blog_name, __json_post.organizations_organization_id, __json_post.organizations_organization_name, __json_post.posts from __json_post) as d limit 1";

        output.WriteLine($"/*expected*/ {expected}");
        output.WriteLine($"/*actual  */ {actual}");

        Assert.Equal(expected, actual);
    }


    [Fact]
    public void TestPostArrayJsonSerialization()
    {
        var query = SelectQueryParser.Parse(JsonTestQuery);

        var queryNode = QueryAstParser.Parse(query);
        queryNode.Where("user_id", w => w.Equal(":user_id"))
            .ToJsonQuery(columnNormalization: true, static x =>
            {
                return x.ArraySerialize("posts", objectName: "posts", upperNode: static x =>
                {
                    return x.Serialize("users", objectName: "user")
                        .Serialize("blogs", objectName: "blog", upperNode: static x =>
                        {
                            return x.Serialize("organizations", objectName: "organization");
                        });
                });
            });

        var actual = queryNode.Query.ToSql();
        output.WriteLine(actual);

        var expected = "with __json as (select posts.post_id as posts__post_id, posts.title as posts__title, posts.content as posts__content, posts.created_at as posts__created_at, users.user_id as users__user_id, users.name as users__user_name, blogs.blog_id as blogs__blog_id, blogs.name as blogs__blog_name, organizations.organization_id as organizations__organization_id, organizations.name as organizations__organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where users.user_id = :user_id), __json_posts as (select json_agg(json_build_object('post_id', __json.posts__post_id, 'title', __json.posts__title, 'content', __json.posts__content, 'created_at', __json.posts__created_at, 'user', json_build_object('user_id', __json.users__user_id, 'user_name', __json.users__user_name), 'blog', json_build_object('blog_id', __json.blogs__blog_id, 'blog_name', __json.blogs__blog_name, 'organization', json_build_object('organization_id', __json.organizations__organization_id, 'organization_name', __json.organizations__organization_name)))) as posts from __json) select row_to_json(d) from (select __json_posts.posts as \"posts\" from __json_posts) as d limit 1";
        Assert.Equal(expected, actual);

        /*
{
  "posts": [
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
    },
    {
      "post_id": 11,
      "title": "Understanding AI",
      "content": "This is a post about AI.",
      "created_at": "2025-03-07T17:53:49.168646",
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
    },
    {
      "post_id": 12,
      "title": "Deep Learning Advances",
      "content": "Exploring the latest in deep learning.",
      "created_at": "2025-03-07T17:53:49.168646",
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
    },
    {
      "post_id": 15,
      "title": "Understanding AI",
      "content": "This is a post about AI.",
      "created_at": "2025-03-07T17:54:06.599913",
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
    },
    {
      "post_id": 16,
      "title": "Deep Learning Advances",
      "content": "Exploring the latest in deep learning.",
      "created_at": "2025-03-07T17:54:06.599913",
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
  ]
}
         */
    }
}
