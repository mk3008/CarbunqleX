using Carbunqlex;
using Carbunqlex.Parsing.Expressions;
using Xunit.Abstractions;

namespace Sample;

/// <summary>
/// This is a sample for editing SELECT statements.
/// You can add, exclude, or modify columns.
/// </summary>
/// <param name="output"></param>
public class SelectEditorSample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    /// <summary>
    /// Modify columns using the GREATEST function to get the maximum value.
    /// </summary>
    [Fact]
    public void SelectValueWithModifyGreatestFunction()
    {
        var query = QueryAstParser.Parse("select s.sale_date, s.sales_amount from sales as s");

        query.ModifyColumn("sale_date", c => c.Greatest(new DateTime(2024, 1, 1)));

        var expected = "select greatest(s.sale_date, '2024-01-01 00:00:00') as sale_date, s.sales_amount from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Exclude columns.
    /// </summary>
    [Fact]
    public void SelectValueWithRemoveColumn()
    {
        var query = QueryAstParser.Parse("select s.sale_date, s.sales_amount from sales as s");

        query.RemoveColumn("sale_date");

        var expected = "select s.sales_amount from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Add columns.
    /// </summary>
    [Fact]
    public void SelectValueWithoutAlias()
    {
        var query = QueryAstParser.Parse("select s.sale_date from sales as s");

        query.AddColumn("s.sales_amount");

        var expected = "select s.sale_date, s.sales_amount from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Add columns with alias.
    /// </summary>
    [Fact]
    public void SelectValueWithAlias()
    {
        var query = QueryAstParser.Parse("select s.sale_date from sales as s");

        query.AddColumn("s.sales_amount", "amount");

        var expected = "select s.sale_date, s.sales_amount as amount from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Add columns with alias.
    /// The column to be added is specified by an expression string.
    /// The written content is parsed through the parser and added to the SELECT clause.
    /// </summary>
    [Fact]
    public void SelectExpressionWithAlias()
    {
        var query = QueryAstParser.Parse("select s.sale_date from sales as s");

        query.AddColumn("1+2", "value");

        var expected = "select s.sale_date, 1 + 2 as value from sales as s";

        var actual = query.ToSql();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// The written content is parsed through the parser and added to the SELECT clause.
    /// If there is a syntax error, an exception will be thrown.
    /// </summary>
    [Fact]
    public void SelectExpressionSyntaxError()
    {
        var query = QueryAstParser.Parse("select s.sale_date from sales as s");

        Assert.Throws<SqlParsingException>(() => query.AddColumn("1 SELECT", "value"));
    }
}
