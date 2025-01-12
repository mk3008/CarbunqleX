using Carbunqlex.ValueExpressions;

namespace Carbunqlex;

public class WhereEditor
{
    private readonly IValueExpression Value;

    private readonly ISelectQuery Query;

    public WhereEditor(ColumnEditor modifier)
    {
        Query = modifier.Query;
        Value = modifier.Value;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    internal void AddCondition(IValueExpression condition)
    {
        if (Query.TryGetWhereClause(out var whereClause))
        {
            whereClause.And(condition);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public WhereEditor Equal(object rightValue)
    {
        AddCondition(Value.Equal(rightValue));
        return this;
    }

    public WhereEditor NotEqual(object rightValue)
    {
        AddCondition(Value.NotEqual(rightValue));
        return this;
    }

    public WhereEditor GreaterThan(object rightValue)
    {
        AddCondition(Value.GreaterThan(rightValue));
        return this;
    }

    public WhereEditor GreaterThanOrEqual(object rightValue)
    {
        AddCondition(Value.GreaterThanOrEqual(rightValue));
        return this;
    }

    public WhereEditor LessThan(object rightValue)
    {
        AddCondition(Value.LessThan(rightValue));
        return this;
    }

    public WhereEditor LessThanOrEqual(object rightValue)
    {
        AddCondition(Value.LessThanOrEqual(rightValue));
        return this;
    }

    public WhereEditor Like(IValueExpression rightValue)
    {
        AddCondition(Value.Like(rightValue));
        return this;
    }

    public WhereEditor Like(object rightValue)
    {
        AddCondition(Value.Like(rightValue));
        return this;
    }

    public WhereEditor NotLike(IValueExpression rightValue)
    {
        AddCondition(Value.NotLike(rightValue));
        return this;
    }

    public WhereEditor NotLike(object rightValue)
    {
        AddCondition(Value.NotLike(rightValue));
        return this;
    }

    public WhereEditor In(params object[] values)
    {
        AddCondition(Value.In(values));
        return this;
    }

    public WhereEditor In(ISelectQuery scalarSubQuery)
    {
        AddCondition(Value.In(scalarSubQuery));
        return this;
    }

    public WhereEditor In(IArgumentExpression rightValue)
    {
        AddCondition(Value.In(rightValue));
        return this;
    }

    public WhereEditor NotIn(params object[] values)
    {
        AddCondition(Value.NotIn(values));
        return this;
    }

    public WhereEditor NotIn(ISelectQuery scalarSubQuery)
    {
        AddCondition(Value.NotIn(scalarSubQuery));
        return this;
    }

    public WhereEditor NotIn(IArgumentExpression rightValue)
    {
        AddCondition(Value.NotIn(rightValue));
        return this;
    }

    public WhereEditor Any(params object[] values)
    {
        AddCondition(Value.Any(values));
        return this;
    }

    public WhereEditor Any(ISelectQuery scalarSubQuery)
    {
        AddCondition(Value.Any(scalarSubQuery));
        return this;
    }

    public WhereEditor Any(IArgumentExpression rightValue)
    {
        AddCondition(Value.Any(rightValue));
        return this;
    }

    public WhereEditor IsNull()
    {
        AddCondition(Value.IsNull());
        return this;
    }

    public WhereEditor IsNotNull()
    {
        AddCondition(Value.IsNotNull());
        return this;
    }

    public WhereEditor Between(object start, object end)
    {
        AddCondition(Value.Between(start, end));
        return this;
    }

    public WhereEditor NotBetween(object start, object end)
    {
        AddCondition(Value.NotBetween(start, end));
        return this;
    }

    public WhereEditor Coalesce(params object[] values)
    {
        var expr = Value.Coalesce(values);
        return new WhereEditor(new ColumnEditor(Query, expr));
    }

    public WhereEditor Coalesce(IEnumerable<object> values)
    {
        var expr = Value.Coalesce(values);
        return new WhereEditor(new ColumnEditor(Query, expr));
    }

    public WhereEditor Greatest(params object[] values)
    {
        var expr = Value.Greatest(values);
        return new WhereEditor(new ColumnEditor(Query, expr));
    }

    public WhereEditor Greatest(IEnumerable<object> values)
    {
        var expr = Value.Greatest(values);
        return new WhereEditor(new ColumnEditor(Query, expr));
    }

    public WhereEditor Least(params object[] values)
    {
        var expr = Value.Least(values);
        return new WhereEditor(new ColumnEditor(Query, expr));
    }

    public WhereEditor Least(IEnumerable<object> values)
    {
        var expr = Value.Least(values);
        return new WhereEditor(new ColumnEditor(Query, expr));
    }
}
