using Carbunqlex;
using Xunit.Abstractions;

namespace Sample;

public class WhereEditorSample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void InjectCondition()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, a.value from table_a as a");

        query.Where("value", w => w.Equal(1));

        //select a.table_a_id, a.value from table_a as a where a.value = 1
        output.WriteLine(query.ToSql());
    }

    [Fact]
    public void InjectConditionByAliasName()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, a.value as price from table_a as a");

        query.Where("price", w => w.Equal(1));

        //select a.table_a_id, a.value as price from table_a as a where a.value = 1
        output.WriteLine(query.ToSql());
    }

    [Fact]
    public void InjectParameterCondition()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, a.value from table_a as a");

        query.Where("value", w => w.Equal(":value"))
            .AddParameter(":value", 1);

        //select a.table_a_id, a.value from table_a as a where a.value = ':value'
        output.WriteLine(query.ToSql());

        // :value = 1
        foreach (var parameter in query.GetParameters())
        {
            output.WriteLine($"{parameter.Key} = {parameter.Value}");
        }
    }

    [Fact]
    public void InjectConditionToCteQuery()
    {
        var query = QueryAstParser.Parse("""
            WITH regional_sales AS (
                SELECT region, SUM(amount) AS total_sales
                FROM orders
                GROUP BY region
            ), top_regions AS (
                SELECT region
                FROM regional_sales
                WHERE total_sales > (SELECT SUM(total_sales)/10 FROM regional_sales)
            )
            SELECT region,
                   product,
                   SUM(quantity) AS product_units,
                   SUM(amount) AS product_sales
            FROM orders
            WHERE region IN (SELECT region FROM top_regions)
            GROUP BY region, product
            """);

        query.Where("region", w => w.Equal("'east'"));

        var expected = "with regional_sales as (select region, SUM(amount) as total_sales from orders where region = 'east' group by region), top_regions as (select region from regional_sales where total_sales > (select SUM(total_sales) / 10 from regional_sales)) select region, product, SUM(quantity) as product_units, SUM(amount) as product_sales from orders where region in (select region from top_regions) group by region, product";

        Assert.Equal(expected, query.ToSql());
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

        Assert.Equal(expected, query.ToSql());
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
                SELECT region, SUM(amount) AS total_sales
                FROM orders
                GROUP BY region
            ), top_regions AS (
                SELECT region
                FROM regional_sales
                WHERE total_sales > (SELECT SUM(total_sales)/10 FROM regional_sales)
            )
            SELECT region,
                   product,
                   SUM(quantity) AS product_units,
                   SUM(amount) AS product_sales
            FROM orders
            WHERE region IN (SELECT region FROM top_regions)
            GROUP BY region, product
            """);

        query.Where("region", w => w.In(userRegionPermissionQuery));

        var expected = "with regional_sales as (select region, SUM(amount) as total_sales from orders where region in (select rrp.region from region_reference_permission as rrp where rrp.user_id = :user_id) group by region), top_regions as (select region from regional_sales where total_sales > (select SUM(total_sales) / 10 from regional_sales)) select region, product, SUM(quantity) as product_units, SUM(amount) as product_sales from orders where region in (select region from top_regions) group by region, product";

        output.WriteLine(query.ToSql());
        Assert.Equal(expected, query.ToSql());
    }
}
