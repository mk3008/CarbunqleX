using Carbunqlex;
using Carbunqlex.Expressions;
using Carbunqlex.Parsing.Expressions;
using Carbunqlex.QuerySources;
using Xunit.Abstractions;

namespace Sample;

/// <summary>  
/// Sample to add table joins.  
/// The target query for the join can be either the current query or the deepest subquery, specified by the isCurrentOnly parameter.
/// </summary>  
/// <param name="output"></param>  
public class FromEditorSample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    /// <summary>  
    /// Sample to add an inner join and add columns from the joined table  
    /// </summary>  
    [Fact]
    public void AddInnerJoin()
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

        query.From("region", isCurrentOnly: true, static from =>
        {
            from.InnerJoin("top_regions", "tp");
            from.EditQuery(q =>
            {
                q.AddColumn("tp.region_name")
                    .GroupBy("region_name");
            });
        });

        // INNER JOIN top_regions AS x ON orders.region = x.region  
        var expected = "with regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales, tp.region_name from orders inner join top_regions as tp on orders.region = tp.region where orders.region in (select x.region from top_regions as x) group by orders.region, orders.product, tp.region_name";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>  
    /// Sample to add a left join and filter by columns from the joined table.  
    /// Set isCurrentOnly: false to use the join as a filter condition  
    /// </summary>  
    [Fact]
    public void AddLeftJoin()
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

        // Set isCurrentOnly: false to use the join as a filter condition  
        query.From("region", isCurrentOnly: false, static from =>
        {
            from.LeftJoin("top_regions", "tp");
            from.EditQuery(q =>
            {
                q.Where("tp.region is null");
            });
        });

        // left join top_regions as tp on orders.region = tp.region where tp.region is null  
        var expected = "with regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders left join top_regions as tp on orders.region = tp.region where tp.region is null group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales from orders where orders.region in (select x.region from top_regions as x) group by orders.region, orders.product";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>  
    /// Sample to add custom join conditions  
    /// </summary>  
    [Fact]
    public void AddCustomJoin()
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

        query.From("region", isCurrentOnly: true, static from =>
        {
            from.Join("inner join",
                new DatasourceExpression(new TableSource("top_regions"), "tp"),
                static (map, ds) =>
                {
                    // map contains the column names and values specified in the search condition  
                    // ds contains the information of the table to be joined  
                    // In this example, the join condition orders.region = tp.region is added  
                    var condition = map.Values
                        .Select(x => x.Equal(new ColumnExpression(ds.Alias, x.DefaultName)))
                        .Aggregate((current, next) => current.And(next));

                    // Add custom join conditions  
                    // In this example, the condition tp.region_name is null is added  
                    condition = condition.And(ValueExpressionParser.Parse($"{ds.Alias}.region_name").IsNull());

                    return condition;
                });
        });

        // on orders.region = tp.region and tp.region_name is null  
        var expected = "with regional_sales as (select orders.region, SUM(orders.amount) as total_sales from orders group by orders.region), top_regions as (select rs.region from regional_sales as rs where rs.total_sales > (select SUM(x.total_sales) / 10 from regional_sales as x)) select orders.region, orders.product, SUM(orders.quantity) as product_units, SUM(orders.amount) as product_sales from orders inner join top_regions as tp on orders.region = tp.region and tp.region_name is null where orders.region in (select x.region from top_regions as x) group by orders.region, orders.product";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }
}
