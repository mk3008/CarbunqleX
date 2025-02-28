using Carbunqlex.Expressions;

namespace Carbunqlex;

public class DatasourceEditor
{
    private readonly ISelectQuery Query;

    private readonly string DatasourceAlias;

    public DatasourceEditor(ISelectQuery query, string datasourceAlias)
    {
        Query = query;
        DatasourceAlias = datasourceAlias;
    }

    public void Edit(Action<DatasourceEditor> action)
    {
        action(this);
    }

    public SelectEditor Select(string columnName)
    {
        return Select(columnName, columnName);
    }

    public SelectEditor Select(string columnName, string columnAlias)
    {
        var expr = new SelectExpression(new ColumnExpression(DatasourceAlias, columnName), columnAlias);
        Query.AddColumn(expr);
        return new SelectEditor(new ColumnEditor(Query, expr.Value, expr));
    }

    public WhereEditor Where(string columnName)
    {
        var keyValues = new Dictionary<string, IValueExpression> { { columnName, new ColumnExpression(DatasourceAlias, columnName) } };
        return new WhereEditor(Query, keyValues);
    }
}
