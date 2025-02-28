using Carbunqlex;
using Xunit.Abstractions;

namespace Sample;

public class SelectEditorSample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ModifyColumnWithGreatest()
    {
        var query = QueryAstParser.Parse("select s.sale_date, s.sales_amount from sales as s");

        query.ModifyColumn("sale_date", c => c.Greatest(new DateTime(2024, 1, 1)));

        var expected = "select greatest(s.sale_date, '2024-01-01 00:00:00') as sale_date, s.sales_amount from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ModifyColumnWithExclude()
    {
        var query = QueryAstParser.Parse("select s.sale_date, s.sales_amount from sales as s");

        query.ModifyColumn("sale_date", c => c.Exclude());

        var expected = "select s.sales_amount from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddSelectColumn()
    {
        var query = QueryAstParser.Parse("select s.sale_date from sales as s");

        query.SelectValue("s.sales_amount");

        var expected = "select s.sale_date, s.sales_amount from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddSelectColumnAlias()
    {
        var query = QueryAstParser.Parse("select s.sale_date from sales as s");

        query.SelectValue("s.sales_amount", "amount");

        var expected = "select s.sale_date, s.sales_amount as amount from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddSelectValue()
    {
        var query = QueryAstParser.Parse("select s.sale_date from sales as s");

        query.SelectValue("1+2", "value");

        var expected = "select s.sale_date, 1 + 2 as value from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }
}
