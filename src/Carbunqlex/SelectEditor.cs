using Carbunqlex.Expressions;

namespace Carbunqlex;

public class SelectEditor
{
    private IValueExpression Value { get; set; }

    private readonly ISelectQuery Query;
    private readonly SelectExpression? SelectExpression;

    public SelectEditor(ColumnEditor queryAccessor)
    {
        Query = queryAccessor.Query;
        Value = queryAccessor.Value;
        SelectExpression = queryAccessor.SelectExpression;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    public SelectEditor Greatest(params object[] values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Greatest(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectEditor Greatest(IEnumerable<object> values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Greatest(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectEditor Least(params object[] values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Least(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectEditor Least(IEnumerable<object> values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Least(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectEditor Coalesce(params object[] values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Coalesce(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectEditor Coalesce(IEnumerable<object> values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Coalesce(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }
}
