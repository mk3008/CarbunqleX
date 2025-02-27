using Carbunqlex;
using Xunit.Abstractions;

namespace Sample;

public class QueryAstParserSample(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ParseSimpleQuery()
    {
        var query = QueryAstParser.Parse("select a.table_a_id, a.value from table_a as a");

        // select a.table_a_id, a.value from table_a as a
        output.WriteLine("* SQL");
        output.WriteLine(query.ToSql());

        output.WriteLine(string.Empty);

        /*
        *Query
         Type: SelectQuery
         Current: select a.table_a_id, a.value from table_a as a
         SelectedColumns: table_a_id, value
          *Datasource
           Type: Table
           Name: a
           Table: table_a
           Columns: table_a_id, value
         */
        output.WriteLine("* AST");
        output.WriteLine(query.ToTreeString());
    }

    [Fact]
    public void ParseComplexQuery()
    {
        var query = QueryAstParser.Parse("""
            with
            dat(line_id, name, unit_price, quantity, tax_rate) as ( 
                values
                (1, 'apple' , 105, 5, 0.07),
                (2, 'orange', 203, 3, 0.07),
                (3, 'banana', 233, 9, 0.07),
                (4, 'tea'   , 309, 7, 0.08),
                (5, 'coffee', 555, 9, 0.08),
                (6, 'cola'  , 456, 2, 0.08)
            ),
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
            """);

        // with dat(line_id, name, unit_price, quantity, tax_rate) as (values (1, 'apple', 105, 5, 0.07), (2, 'orange', 203, 3, 0.07), (3, 'banana', 233, 9, 0.07), (4, 'tea', 309, 7, 0.08), (5, 'coffee', 555, 9, 0.08), (6, 'cola', 456, 2, 0.08)), detail as (select q.*, trunc(q.price * (1 + q.tax_rate)) - q.price as tax, q.price * (1 + q.tax_rate) - q.price as raw_tax from (select dat.*, (dat.unit_price * dat.quantity) as price from dat) as q), tax_summary as (select d.tax_rate, trunc(sum(raw_tax)) as total_tax from detail as d group by d.tax_rate) select line_id, name, unit_price, quantity, tax_rate, price, price + tax as tax_included_price, tax from (select line_id, name, unit_price, quantity, tax_rate, price, tax + adjust_tax from (select q.*, case when q.total_tax - q.cumulative >= q.priority then 1 else 0 end as adjust_tax from (select d.*, s.total_tax, sum(d.tax) over(partition by d.tax_rate) as cumulative, row_number() over(partition by d.tax_rate order by d.raw_tax % 1 desc, d.line_id) as priority from detail as d inner join tax_summary as s on d.tax_rate = s.tax_rate) as q) as q) as q order by line_id
        output.WriteLine("* SQL");
        output.WriteLine(query.ToSql());

        output.WriteLine(string.Empty);

        /*
        *Query
         Type: SelectQuery
         Current: select line_id, name, unit_price, quantity, tax_rate, price, price + tax as tax_included_price, tax from (select line_id, name, unit_price, quantity, tax_rate, price, tax + adjust_tax from (select q.*, case when q.total_tax - q.cumulative >= q.priority then 1 else 0 end as adjust_tax from (select d.*, s.total_tax, sum(d.tax) over(partition by d.tax_rate) as cumulative, row_number() over(partition by d.tax_rate order by d.raw_tax % 1 desc, d.line_id) as priority from detail as d inner join tax_summary as s on d.tax_rate = s.tax_rate) as q) as q) as q order by line_id
         SelectedColumns: line_id, name, unit_price, quantity, tax_rate, price, tax_included_price, tax
          *Datasource
           Type: SubQuery
           Name: q
           Table: 
           Columns: line_id, name, unit_price, quantity, tax_rate, price, tax
            *Query
             Type: SelectQuery
             Current: select line_id, name, unit_price, quantity, tax_rate, price, tax + adjust_tax from (select q.*, case when q.total_tax - q.cumulative >= q.priority then 1 else 0 end as adjust_tax from (select d.*, s.total_tax, sum(d.tax) over(partition by d.tax_rate) as cumulative, row_number() over(partition by d.tax_rate order by d.raw_tax % 1 desc, d.line_id) as priority from detail as d inner join tax_summary as s on d.tax_rate = s.tax_rate) as q) as q
             SelectedColumns: line_id, name, unit_price, quantity, tax_rate, price, tax
              *Datasource
               Type: SubQuery
               Name: q
               Table: 
               Columns: adjust_tax
                *Query
                 Type: SelectQuery
                 Current: select q.*, case when q.total_tax - q.cumulative >= q.priority then 1 else 0 end as adjust_tax from (select d.*, s.total_tax, sum(d.tax) over(partition by d.tax_rate) as cumulative, row_number() over(partition by d.tax_rate order by d.raw_tax % 1 desc, d.line_id) as priority from detail as d inner join tax_summary as s on d.tax_rate = s.tax_rate) as q
                 SelectedColumns: *, adjust_tax
                  *Datasource
                   Type: SubQuery
                   Name: q
                   Table: 
                   Columns: total_tax, cumulative, priority
                    *Query
                     Type: SelectQuery
                     Current: select d.*, s.total_tax, sum(d.tax) over(partition by d.tax_rate) as cumulative, row_number() over(partition by d.tax_rate order by d.raw_tax % 1 desc, d.line_id) as priority from detail as d inner join tax_summary as s on d.tax_rate = s.tax_rate
                     SelectedColumns: *, total_tax, cumulative, priority
                      *Datasource
                       Type: CommonTableExtension
                       Name: d
                       Table: detail
                       Columns: *, tax, raw_tax
                        *Query
                         Type: SelectQuery
                         Current: select q.*, trunc(q.price * (1 + q.tax_rate)) - q.price as tax, q.price * (1 + q.tax_rate) - q.price as raw_tax from (select dat.*, (dat.unit_price * dat.quantity) as price from dat) as q
                         SelectedColumns: *, tax, raw_tax
                          *Datasource
                           Type: SubQuery
                           Name: q
                           Table: 
                           Columns: price
                            *Query
                             Type: SelectQuery
                             Current: select dat.*, (dat.unit_price * dat.quantity) as price from dat
                             SelectedColumns: *, price
                              *Datasource
                               Type: CommonTableExtension
                               Name: dat
                               Table: dat
                               Columns: line_id, name, unit_price, quantity, tax_rate
                                *Query
                                 Type: ValuesQuery
                                 Current: values (1, 'apple', 105, 5, 0.07), (2, 'orange', 203, 3, 0.07), (3, 'banana', 233, 9, 0.07), (4, 'tea', 309, 7, 0.08), (5, 'coffee', 555, 9, 0.08), (6, 'cola', 456, 2, 0.08)
                                 SelectedColumns: 
                      *Datasource
                       Type: CommonTableExtension
                       Name: s
                       Table: tax_summary
                       Columns: tax_rate, total_tax
                        *Query
                         Type: SelectQuery
                         Current: select d.tax_rate, trunc(sum(raw_tax)) as total_tax from detail as d group by d.tax_rate
                         SelectedColumns: tax_rate, total_tax
                          *Datasource
                           Type: CommonTableExtension
                           Name: d
                           Table: detail
                           Columns: *, tax, raw_tax
                            *Query
                             Type: SelectQuery
                             Current: select q.*, trunc(q.price * (1 + q.tax_rate)) - q.price as tax, q.price * (1 + q.tax_rate) - q.price as raw_tax from (select dat.*, (dat.unit_price * dat.quantity) as price from dat) as q
                             SelectedColumns: *, tax, raw_tax
                              *Datasource
                               Type: SubQuery
                               Name: q
                               Table: 
                               Columns: price
                                *Query
                                 Type: SelectQuery
                                 Current: select dat.*, (dat.unit_price * dat.quantity) as price from dat
                                 SelectedColumns: *, price
                                  *Datasource
                                   Type: CommonTableExtension
                                   Name: dat
                                   Table: dat
                                   Columns: line_id, name, unit_price, quantity, tax_rate
                                    *Query
                                     Type: ValuesQuery
                                     Current: values (1, 'apple', 105, 5, 0.07), (2, 'orange', 203, 3, 0.07), (3, 'banana', 233, 9, 0.07), (4, 'tea', 309, 7, 0.08), (5, 'coffee', 555, 9, 0.08), (6, 'cola', 456, 2, 0.08)
                                     SelectedColumns: 
         */
        output.WriteLine("* AST");
        output.WriteLine(query.ToTreeString());
    }
}
