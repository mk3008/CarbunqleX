using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class TreeTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Simple()
    {
        var query = SelectQueryParser.Parse("select u.user_id, u.users_name from users as u");

        var queryNode = QueryNodeFactory.Create(query);
        var actual = queryNode.ToTreeString();
        output.WriteLine(actual);

        var expected = """
           *Query
            Type: SelectQuery
            Current: select u.user_id, u.users_name from users as u
            SelectedColumns: user_id, users_name
             *Datasource
              Type: Table
              Name: u
              Table: users
              Columns: user_id, users_name
           """;

        Assert.Equal(expected, actual, ignoreWhiteSpaceDifferences: true);
    }

    [Fact]
    public void SubQuery()
    {
        var query = SelectQueryParser.Parse("select * from (select u.user_id, u.users_name from users as u) as d");

        var queryNode = QueryNodeFactory.Create(query);
        var actual = queryNode.ToTreeString();
        output.WriteLine(actual);

        var expected =
"""
*Query
 Type: SelectQuery
 Current: select * from (select u.user_id, u.users_name from users as u) as d
 SelectedColumns: *
  *Datasource
   Type: SubQuery
   Name: d
   Table: 
   Columns: user_id, users_name
    *Query
     Type: SelectQuery
     Current: select u.user_id, u.users_name from users as u
     SelectedColumns: user_id, users_name
      *Datasource
       Type: Table
       Name: u
       Table: users
       Columns: user_id, users_name
""";

        Assert.Equal(expected, actual, ignoreWhiteSpaceDifferences: true);
    }

    [Fact]
    public void Join()
    {
        var query = SelectQueryParser.Parse("""
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

        var queryNode = QueryNodeFactory.Create(query);
        var actual = queryNode.ToTreeString();
        output.WriteLine(actual);

        var expected = """
            *Query
             Type: SelectQuery
             Current: select posts.post_id, posts.title, posts.content, posts.created_at, users.user_id, users.name as user_name, blogs.blog_id, blogs.name as blog_name, organizations.organization_id, organizations.name as organization_name from posts inner join users on posts.user_id = users.user_id inner join blogs on posts.blog_id = blogs.blog_id inner join organizations on blogs.organization_id = organizations.organization_id where posts.post_id = :post_id
             SelectedColumns: post_id, title, content, created_at, user_id, user_name, blog_id, blog_name, organization_id, organization_name
              *Datasource
               Type: Table
               Name: posts
               Table: posts
               Columns: post_id, title, content, created_at, user_id, blog_id
              *Datasource
               Type: Table
               Name: users
               Table: users
               Columns: user_id, name
              *Datasource
               Type: Table
               Name: blogs
               Table: blogs
               Columns: blog_id, name, organization_id
              *Datasource
               Type: Table
               Name: organizations
               Table: organizations
               Columns: organization_id, name
            """;

        Assert.Equal(expected, actual);
    }
}
