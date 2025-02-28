using Carbunqlex;
using Xunit.Abstractions;

namespace Sample;

public class WhereEditorSample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;


    [Fact]
    public void Default()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, a.value from table_a as a");

        query.Where("a.value = 1");

        var expected = "select a.table_a_id, a.value from table_a as a where a.value = 1";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InjectCondition()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, a.value from table_a as a");

        query.Where("value", w => w.Equal(1));

        var expected = "select a.table_a_id, a.value from table_a as a where a.value = 1";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InjectConditionByAliasName()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, a.value as price from table_a as a");

        query.Where("price", w => w.Equal(1));

        var expected = "select a.table_a_id, a.value as price from table_a as a where a.value = 1";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InjectParameterCondition()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, a.value from table_a as a");

        query.Where("value", w => w.Equal(":value"))
            .AddParameter(":value", 1);

        var expected = "select a.table_a_id, a.value from table_a as a where a.value = :value";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);

        var parameters = query.GetParameters();
        Assert.Single(parameters);
        Assert.Equal(":value", parameters.First().Key);
        Assert.Equal(1, parameters.First().Value);
    }

    [Fact]
    public void InjectConditionToCteQueryDefault()
    {
        var query = QueryAstParser.Parse("""
            WITH regional_sales AS (
                SELECT orders.region, SUM(orders.amount) AS total_sales
                FROM orders
                GROUP BY orders.region
            ), top_regions AS (
                SELECT rs.region
                FROM regional_sales rs
                WHERE rs.total_sales > (SELECT SUM(x.total_sales)/10 FROM regional_sales x)
            )
            SELECT orders.region,
                   orders.product,
                   SUM(orders.quantity) AS product_units,
                   SUM(orders.amount) AS product_sales
            FROM orders 
            WHERE orders.region IN (SELECT x.region FROM top_regions x)
            GROUP BY orders.region, orders.product
            """);

        query.Where("region = 'east'");

        var expected = "with regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales from orders where orders.region in (select x.region from top_regions as x) and region = 'east' group by orders.region, orders.product";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InjectConditionToCteQuery()
    {
        var query = QueryAstParser.Parse("""
            WITH regional_sales AS (
                SELECT orders.region, SUM(orders.amount) AS total_sales
                FROM orders
                GROUP BY orders.region
            ), top_regions AS (
                SELECT rs.region
                FROM regional_sales rs
                WHERE rs.total_sales > (SELECT SUM(x.total_sales)/10 FROM regional_sales x)
            )
            SELECT orders.region,
                   orders.product,
                   SUM(orders.quantity) AS product_units,
                   SUM(orders.amount) AS product_sales
            FROM orders 
            WHERE orders.region IN (SELECT x.region FROM top_regions x)
            GROUP BY orders.region, orders.product
            """);

        query.Where("region", w => w.Equal("'east'"));

        var expected = "with regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders where orders.region = 'east' group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales from orders where orders.region in (select x.region from top_regions as x) group by orders.region, orders.product";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InjectConditionToUnionQuery()
    {
        var query = QueryAstParser.Parse("""
            SELECT user_id, name, email, 'user' AS type
            FROM users
            UNION
            SELECT customer_id, name, email, 'customer' AS type
            FROM customers
            """);

        query.Where("name", w => w.Like("'%mike%'"));

        var expected = "select user_id, name, email, 'user' as type from users where name like '%mike%' UNION select customer_id, name, email, 'customer' as type from customers where name like '%mike%'";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InjectConditionWithSubquery()
    {
        var userRegionPermissionQuery = QueryAstParser.Parse("""
            SELECT rrp.region
            FROM region_reference_permission rrp
            WHERE rrp.user_id = :user_id
            """);

        var query = QueryAstParser.Parse("""
            WITH regional_sales AS (
                SELECT orders.region, SUM(orders.amount) AS total_sales
                FROM orders
                GROUP BY orders.region
            ), top_regions AS (
                SELECT rs.region
                FROM regional_sales rs
                WHERE rs.total_sales > (SELECT SUM(x.total_sales)/10 FROM regional_sales x)
            )
            SELECT orders.region,
                   orders.product,
                   SUM(orders.quantity) AS product_units,
                   SUM(orders.amount) AS product_sales
            FROM orders 
            WHERE orders.region IN (SELECT x.region FROM top_regions x)
            GROUP BY orders.region, orders.product
            """);

        query.Where("region", w => w.In(userRegionPermissionQuery));

        var expected = "with regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders where orders.region in (select x.region from (select rrp.region from region_reference_permission as rrp where rrp.user_id = :user_id) as x) group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales from orders where orders.region in (select x.region from top_regions as x) group by orders.region, orders.product";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InjectExistsConditionWithSubquery()
    {
        var userRegionPermissionQuery = QueryAstParser.Parse("""
            SELECT rrp.region
            FROM region_reference_permission rrp
            WHERE rrp.user_id = :user_id
            """);

        var query = QueryAstParser.Parse("""
            WITH regional_sales AS (
                SELECT orders.region, SUM(orders.amount) AS total_sales
                FROM orders
                GROUP BY orders.region
            ), top_regions AS (
                SELECT rs.region
                FROM regional_sales rs
                WHERE rs.total_sales > (SELECT SUM(x.total_sales)/10 FROM regional_sales x)
            )
            SELECT orders.region,
                   orders.product,
                   SUM(orders.quantity) AS product_units,
                   SUM(orders.amount) AS product_sales
            FROM orders 
            WHERE orders.region IN (SELECT x.region FROM top_regions x)
            GROUP BY orders.region, orders.product
            """);

        query.Where("region", w => w.Exists(userRegionPermissionQuery));

        var expected = "with regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders where exists (select * from (select rrp.region from region_reference_permission as rrp where rrp.user_id = :user_id) as x where orders.region = x.region) group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales from orders where orders.region in (select x.region from top_regions as x) group by orders.region, orders.product";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InjectExistsConditionWithCteSubquery()
    {
        var userRegionPermissionQuery = QueryAstParser.Parse("""
            WITH user_permissions AS (
                SELECT rrp.region
                FROM region_reference_permission rrp
                WHERE rrp.user_id = :user_id
            )
            SELECT up.region
            FROM user_permissions up
            """);

        var query = QueryAstParser.Parse("""
            WITH regional_sales AS (
                SELECT orders.region, SUM(orders.amount) AS total_sales
                FROM orders
                GROUP BY orders.region
            ), top_regions AS (
                SELECT rs.region
                FROM regional_sales rs
                WHERE rs.total_sales > (SELECT SUM(x.total_sales)/10 FROM regional_sales x)
            )
            SELECT orders.region,
                   orders.product,
                   SUM(orders.quantity) AS product_units,
                   SUM(orders.amount) AS product_sales
            FROM orders 
            WHERE orders.region IN (SELECT x.region FROM top_regions x)
            GROUP BY orders.region, orders.product
            """);

        query.Where("region", w => w.Exists(userRegionPermissionQuery));

        var expected = "with user_permissions as (select rrp.region from region_reference_permission as rrp where rrp.user_id = :user_id), regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders where exists (select * from (select up.region from user_permissions as up) as x where orders.region = x.region) group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales from orders where orders.region in (select x.region from top_regions as x) group by orders.region, orders.product";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }
}
