using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Carbunql;
using Carbunqlex.Parsing;
using SqModel.Analysis;

public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<SimpleSelectQueryParseBenchmark>();
    }
}

public class SimpleSelectQueryParseBenchmark
{
    private readonly string QueryString = "select id, name from users";

    [Benchmark]
    public string SqModel()
    {
        var sq = SqlParser.Parse(QueryString);
        return sq.ToQuery().CommandText;
    }

    [Benchmark]
    public string Carbunql()
    {
        var sq = new Carbunql.SelectQuery(QueryString);
        return sq.ToOneLineText();
    }

    [Benchmark]
    public string Carbunqlex()
    {
        var tokenizer = new SqlTokenizer(QueryString);
        var sq = SelectQueryParser.Parse(tokenizer);
        return sq.ToSql();
    }
}
