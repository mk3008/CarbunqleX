using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex;

public class SelectModifier
{
    private IValueExpression Value { get; set; }

    private readonly ISelectQuery Query;
    private readonly SelectExpression? SelectExpression;

    public SelectModifier(ColumnModifier queryAccessor)
    {
        Query = queryAccessor.Query;
        Value = queryAccessor.Value;
        SelectExpression = queryAccessor.SelectExpression;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    public SelectModifier Greatest(params object[] values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Greatest(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectModifier Greatest(IEnumerable<object> values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Greatest(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectModifier Least(params object[] values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Least(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectModifier Least(IEnumerable<object> values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Least(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectModifier Coalesce(params object[] values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Coalesce(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public SelectModifier Coalesce(IEnumerable<object> values)
    {
        if (SelectExpression != null)
        {
            var expr = ValueBuilder.Coalesce(new object[] { Value }.Union(values));
            SelectExpression.Value = expr;
            Value = expr;
        }
        return this;
    }

    public void Remove()
    {
        if (SelectExpression != null)
        {
            Query.RemoveColumn(SelectExpression);
        }
    }
}
