using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Carbunql;
using Carbunqlex.Parsing;
using SqModel.Analysis;

public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<SelectQueryParseBenchmark>();
    }
}
public class SelectQueryParseBenchmark
{
    private readonly string ShortSelect = "select id, name from users";
    private readonly string JoinSelect = "select u.id, o.total from users u join orders o on u.id = o.user_id";
    private readonly string GroupBySelect = "select category, count(*) from products group by category having count(*) > 10";
    private readonly string SubquerySelect = "select id, (select count(*) from orders o where o.user_id = u.id) as order_count from users u";
    private readonly string CteSelect = "with cte as (select id from users where active = true) select * from cte";
    private readonly string UnionSelect = "select id, name from users union select id, name from admins";
    private readonly string OrderBySelect = "select id, name from users order by created_at desc limit 10";

    private readonly string LongQuery = """
        with
        detail as (
            select  
                q.*,
                trunc(q.price * (1 + q.tax_rate)) - q.price as tax,
                q.price * (1 + q.tax_rate) - q.price as raw_tax
            from
                (
                    select
                        dat.*,
                        (dat.unit_price * dat.quantity) as price
                    from
                        dat
                ) q
        ), 
        tax_summary as (
            select
                d.tax_rate,
                trunc(sum(raw_tax)) as total_tax
            from
                detail d
            group by
                d.tax_rate
        )
        select 
           line_id,
            name,
            unit_price,
            quantity,
            tax_rate,
            price,
            price + tax as tax_included_price,
            tax
        from
            (
                select
                    line_id,
                    name,
                    unit_price,
                    quantity,
                    tax_rate,
                    price,
                    tax + adjust_tax as tax
                from
                    (
                        select
                            q.*,
                            case when q.total_tax - q.cumulative >= q.priority then 1 else 0 end as adjust_tax
                        from
                            (
                                select  
                                    d.*, 
                                    s.total_tax,
                                    sum(d.tax) over (partition by d.tax_rate) as cumulative,
                                    row_number() over (partition by d.tax_rate order by d.raw_tax % 1 desc, d.line_id) as priority
                                from
                                    detail d
                                    inner join tax_summary s on d.tax_rate = s.tax_rate
                            ) q
                    ) q
            ) q
        order by 
            line_id
        """;

    [Benchmark] public string SqModel_Shor() => ParseWithSqModel(ShortSelect);
    [Benchmark] public string Carbunql_Short() => ParseWithCarbunql(ShortSelect);
    [Benchmark] public string Carbunqlex_Short() => ParseWithCarbunqlex(ShortSelect);

    [Benchmark] public string SqModel_Join() => ParseWithSqModel(JoinSelect);
    [Benchmark] public string Carbunql_Join() => ParseWithCarbunql(JoinSelect);
    [Benchmark] public string Carbunqlex_Join() => ParseWithCarbunqlex(JoinSelect);

    [Benchmark] public string SqModel_GroupBy() => ParseWithSqModel(GroupBySelect);
    [Benchmark] public string Carbunql_GroupBy() => ParseWithCarbunql(GroupBySelect);
    [Benchmark] public string Carbunqlex_GroupBy() => ParseWithCarbunqlex(GroupBySelect);

    [Benchmark] public string SqModel_Subquery() => ParseWithSqModel(SubquerySelect);
    [Benchmark] public string Carbunql_Subquery() => ParseWithCarbunql(SubquerySelect);
    [Benchmark] public string Carbunqlex_Subquery() => ParseWithCarbunqlex(SubquerySelect);

    [Benchmark] public string SqModel_Cte() => ParseWithSqModel(CteSelect);
    [Benchmark] public string Carbunql_Cte() => ParseWithCarbunql(CteSelect);
    [Benchmark] public string Carbunqlex_Cte() => ParseWithCarbunqlex(CteSelect);

    [Benchmark] public string SqModel_Union() => ParseWithSqModel(UnionSelect);
    [Benchmark] public string Carbunql_Union() => ParseWithCarbunql(UnionSelect);
    [Benchmark] public string Carbunqlex_Union() => ParseWithCarbunqlex(UnionSelect);

    [Benchmark] public string SqModel_OrderBy() => ParseWithSqModel(OrderBySelect);
    [Benchmark] public string Carbunql_OrderBy() => ParseWithCarbunql(OrderBySelect);
    [Benchmark] public string Carbunqlex_OrderBy() => ParseWithCarbunqlex(OrderBySelect);

    [Benchmark] public string SqModel_Long() => ParseWithSqModel(LongQuery);
    [Benchmark] public string Carbunql_Long() => ParseWithCarbunql(LongQuery);
    [Benchmark] public string Carbunqlex_Long() => ParseWithCarbunqlex(LongQuery);

    private string ParseWithSqModel(string query)
    {
        var sq = SqlParser.Parse(query);
        return sq.ToQuery().CommandText;
    }

    private string ParseWithCarbunql(string query)
    {
        var sq = new Carbunql.SelectQuery(query);
        return sq.ToOneLineText();
    }

    private string ParseWithCarbunqlex(string query)
    {
        var tokenizer = new SqlTokenizer(query);
        var sq = SelectQueryParser.Parse(tokenizer);
        return sq.ToSql();
    }
}

