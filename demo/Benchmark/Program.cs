using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

public class Program
{
    public static void Main()
    {
        //var benchmark = new SelectQueryParseBenchmark();
        //benchmark.PrintTokenCounts();
        //Console.WriteLine(benchmark.SqlScriptDOM_Parse_Tokens_230());

        BenchmarkRunner.Run<SelectQueryParseBenchmark>();
    }
}
public class SelectQueryParseBenchmark
{
    private static readonly TSql150Parser Parser = new TSql150Parser(false);

    private Sql150ScriptGenerator Generator = new Sql150ScriptGenerator(new SqlScriptGeneratorOptions
    {
        AlignClauseBodies = false,
        AsKeywordOnOwnLine = false,
        IndentationSize = 0,
        IncludeSemicolons = false,
        MultilineSelectElementsList = false,
        MultilineWherePredicatesList = false,
        NewLineBeforeCloseParenthesisInMultilineList = false,
        NewLineBeforeFromClause = false,
        NewLineBeforeGroupByClause = false,
        NewLineBeforeHavingClause = false,
        NewLineBeforeJoinClause = false,
        NewLineBeforeOffsetClause = false,
        NewLineBeforeOpenParenthesisInMultilineList = false,
        NewLineBeforeOrderByClause = false,
        NewLineBeforeOutputClause = false,
        NewLineBeforeWhereClause = false
    });

    private readonly string Tokens20 = """
        SELECT id, name, email, age, created_at, updated_at, status, role, last_login, country 
        FROM users 
        WHERE id = 1;
        """;
    private readonly string Tokens70 = """
        SELECT 
            u.id, u.name, u.email, u.age, u.status, u.role, 
            o.id AS order_id, o.total, o.order_date, o.status AS order_status 
        FROM users AS u 
        JOIN orders AS o ON u.id = o.user_id 
        WHERE u.age > 1 AND o.status = 'completed' 
        ORDER BY o.order_date DESC;
        """;

    private readonly string Tokens140 = """
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
        HAVING SUM(o.total) > 10
        ORDER BY total_spent DESC;        
        """;

    private readonly string Tokens230 = """
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

    [Benchmark] public void Carbunqlex_ParseOnly_Tokens_20() => ParseOnlyWithCarbunqlex(Tokens20);
    [Benchmark] public void SqlScriptDOM_ParseOnly_Tokens_20() => ParseOnlyWithSqlScriptDOM(Tokens20);

    [Benchmark] public void Carbunqlex_ParseOnly_Tokens_70() => ParseOnlyWithCarbunqlex(Tokens70);
    [Benchmark] public void SqlScriptDOM_ParseOnly_Tokens_70() => ParseOnlyWithSqlScriptDOM(Tokens70);

    [Benchmark] public void Carbunqlex_ParseOnly_Tokens_140() => ParseOnlyWithCarbunqlex(Tokens140);
    [Benchmark] public void SqlScriptDOM_ParseOnly_Tokens_140() => ParseOnlyWithSqlScriptDOM(Tokens140);

    [Benchmark] public void Carbunqlex_ParseOnly_Tokens_230() => ParseOnlyWithCarbunqlex(Tokens230);
    [Benchmark] public void SqlScriptDOM_ParseOnly_Tokens_230() => ParseOnlyWithSqlScriptDOM(Tokens230);


    [Benchmark] public string Carbunqlex_Parse_Tokens_20() => ParseWithCarbunqlex(Tokens20);
    [Benchmark] public string SqlScriptDOM_Parse_Tokens_20() => ParseWithSqlScriptDOM(Tokens20);

    [Benchmark] public string Carbunqlex_Parse_Tokens_70() => ParseWithCarbunqlex(Tokens70);
    [Benchmark] public string SqlScriptDOM_Parse_Tokens_70() => ParseWithSqlScriptDOM(Tokens70);

    [Benchmark] public string Carbunqlex_Parse_Tokens_140() => ParseWithCarbunqlex(Tokens140);
    [Benchmark] public string SqlScriptDOM_Parse_Tokens_140() => ParseWithSqlScriptDOM(Tokens140);

    [Benchmark] public string Carbunqlex_Parse_Tokens_230() => ParseWithCarbunqlex(Tokens230);
    [Benchmark] public string SqlScriptDOM_Parse_Tokens_230() => ParseWithSqlScriptDOM(Tokens230);

    private void ParseOnlyWithCarbunqlex(string query)
    {
        var tokenizer = new SqlTokenizer(query);
        var sq = SelectQueryParser.Parse(tokenizer);
    }

    private string ParseWithCarbunqlex(string query)
    {
        var tokenizer = new SqlTokenizer(query);
        var sq = SelectQueryParser.Parse(tokenizer);
        return sq.ToSql();
    }

    private void ParseOnlyWithSqlScriptDOM(string query)
    {
        using var reader = new StringReader(query);
        Parser.Parse(reader, out var errors);
    }

    private string ParseWithSqlScriptDOM(string query)
    {
        using var reader = new StringReader(query);
        var fragment = Parser.Parse(reader, out var errors);

        Generator.GenerateScript(fragment, out string script);
        return script;
    }

    private int CountTokens(string sql)
    {
        var tokenizer = new SqlTokenizer(sql);
        int count = 0;
        while (tokenizer.TryRead(out var token))
        {
            count++;
        }
        return count;
    }

    public void PrintTokenCounts()
    {
        Console.WriteLine($"CarbunleX Tokens[Short]: {CountTokens(Tokens20)} tokens");
        Console.WriteLine($"CarbunleX Tokens[Middle]: {CountTokens(Tokens70)} tokens");
        Console.WriteLine($"CarbunleX Tokens[Long]: {CountTokens(Tokens140)} tokens");
        Console.WriteLine($"CarbunleX Tokens[SuperLong]: {CountTokens(Tokens230)} tokens");
    }
}
