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
    private readonly string Short = """
        SELECT id, name, email, age, created_at, updated_at, status, role, last_login, country 
        FROM users 
        WHERE id = :id;
        """;
    private readonly string Middle = """
        SELECT 
            u.id, u.name, u.email, u.age, u.status, u.role, 
            o.id AS order_id, o.total, o.order_date, o.status AS order_status 
        FROM users AS u 
        JOIN orders AS o ON u.id = o.user_id 
        WHERE u.age > :age AND o.status = 'completed' 
        ORDER BY o.order_date DESC 
        LIMIT 10;
        """;

    private readonly string Long = """
        WITH recent_orders AS (
            SELECT user_id, MAX(order_date) AS last_order 
            FROM orders 
            GROUP BY user_id
        )
        SELECT 
            u.id, u.name, u.email, u.age, u.status, u.role, u.created_at, u.updated_at, 
            r.last_order, SUM(o.total) AS total_spent 
        FROM users AS u
        JOIN orders AS o ON u.id = o.user_id
        JOIN recent_orders AS r ON u.id = r.user_id
        WHERE u.status = 'active'
        GROUP BY u.id, u.name, u.email, u.age, u.status, u.role, u.created_at, u.updated_at, r.last_order
        HAVING SUM(o.total) > :threshold
        ORDER BY total_spent DESC;        
        """;

    private readonly string SuperLong = """
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

    [Benchmark] public string SqModel_Parse_Short() => ParseWithSqModel(Short);
    [Benchmark] public string Carbunql_Parse_Short() => ParseWithCarbunql(Short);
    [Benchmark] public string Carbunqlex_Parse_Short() => ParseWithCarbunqlex(Short);
    [Benchmark] public void Carbunqlex_ParseOnly_Short() => ParseOnlyWithCarbunqlex(Short);

    [Benchmark] public string SqModel_Parse_Middle() => ParseWithSqModel(Middle);
    [Benchmark] public string Carbunql_Parse_Middle() => ParseWithCarbunql(Middle);
    [Benchmark] public string Carbunqlex_Parse_Middle() => ParseWithCarbunqlex(Middle);
    [Benchmark] public void Carbunqlex_ParseOnly_Middle() => ParseOnlyWithCarbunqlex(Middle);

    [Benchmark] public string SqModel_Parse_Long() => ParseWithSqModel(Long);
    [Benchmark] public string Carbunql_Parse_Long() => ParseWithCarbunql(Long);
    [Benchmark] public string Carbunqlex_Parse_Long() => ParseWithCarbunqlex(Long);
    [Benchmark] public void Carbunqlex_ParseOnly_Long() => ParseOnlyWithCarbunqlex(Long);

    [Benchmark] public string SqModel_Parse_SuperLong() => ParseWithSqModel(SuperLong);
    [Benchmark] public string Carbunql_Parse_SuperLong() => ParseWithCarbunql(SuperLong);
    [Benchmark] public string Carbunqlex_Parse_SuperLong() => ParseWithCarbunqlex(SuperLong);
    [Benchmark] public void Carbunqlex_ParseOnly_SuperLong() => ParseOnlyWithCarbunqlex(SuperLong);

    private void ParseOnlyWithCarbunqlex(string query)
    {
        var tokenizer = new SqlTokenizer(query);
        var sq = SelectQueryParser.Parse(tokenizer);
    }

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
