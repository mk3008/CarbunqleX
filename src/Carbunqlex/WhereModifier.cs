using Carbunqlex.ValueExpressions;

namespace Carbunqlex;

public class WhereModifier
{
    private readonly IValueExpression Value;

    private readonly ISelectQuery Query;

    public WhereModifier(ColumnModifier modifier)
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

    public WhereModifier Equal(object rightValue)
    {
        AddCondition(Value.Equal(rightValue));
        return this;
    }

    public WhereModifier NotEqual(object rightValue)
    {
        AddCondition(Value.NotEqual(rightValue));
        return this;
    }

    public WhereModifier GreaterThan(object rightValue)
    {
        AddCondition(Value.GreaterThan(rightValue));
        return this;
    }

    public WhereModifier GreaterThanOrEqual(object rightValue)
    {
        AddCondition(Value.GreaterThanOrEqual(rightValue));
        return this;
    }

    public WhereModifier LessThan(object rightValue)
    {
        AddCondition(Value.LessThan(rightValue));
        return this;
    }

    public WhereModifier LessThanOrEqual(object rightValue)
    {
        AddCondition(Value.LessThanOrEqual(rightValue));
        return this;
    }

    public WhereModifier Like(IValueExpression rightValue)
    {
        AddCondition(Value.Like(rightValue));
        return this;
    }

    public WhereModifier Like(object rightValue)
    {
        AddCondition(Value.Like(rightValue));
        return this;
    }

    public WhereModifier NotLike(IValueExpression rightValue)
    {
        AddCondition(Value.NotLike(rightValue));
        return this;
    }

    public WhereModifier NotLike(object rightValue)
    {
        AddCondition(Value.NotLike(rightValue));
        return this;
    }

    public WhereModifier In(params object[] values)
    {
        AddCondition(Value.In(values));
        return this;
    }

    public WhereModifier In(ISelectQuery scalarSubQuery)
    {
        AddCondition(Value.In(scalarSubQuery));
        return this;
    }

    public WhereModifier In(IArgumentExpression rightValue)
    {
        AddCondition(Value.In(rightValue));
        return this;
    }

    public WhereModifier NotIn(params object[] values)
    {
        AddCondition(Value.NotIn(values));
        return this;
    }

    public WhereModifier NotIn(ISelectQuery scalarSubQuery)
    {
        AddCondition(Value.NotIn(scalarSubQuery));
        return this;
    }

    public WhereModifier NotIn(IArgumentExpression rightValue)
    {
        AddCondition(Value.NotIn(rightValue));
        return this;
    }

    public WhereModifier Any(params object[] values)
    {
        AddCondition(Value.Any(values));
        return this;
    }

    public WhereModifier Any(ISelectQuery scalarSubQuery)
    {
        AddCondition(Value.Any(scalarSubQuery));
        return this;
    }

    public WhereModifier Any(IArgumentExpression rightValue)
    {
        AddCondition(Value.Any(rightValue));
        return this;
    }

    public WhereModifier IsNull()
    {
        AddCondition(Value.IsNull());
        return this;
    }

    public WhereModifier IsNotNull()
    {
        AddCondition(Value.IsNotNull());
        return this;
    }

    public WhereModifier Between(object start, object end)
    {
        AddCondition(Value.Between(start, end));
        return this;
    }

    public WhereModifier NotBetween(object start, object end)
    {
        AddCondition(Value.NotBetween(start, end));
        return this;
    }

    public WhereModifier Coalesce(params object[] values)
    {
        var expr = Value.Coalesce(values);
        return new WhereModifier(new ColumnModifier(Query, expr));
    }

    public WhereModifier Coalesce(IEnumerable<object> values)
    {
        var expr = Value.Coalesce(values);
        return new WhereModifier(new ColumnModifier(Query, expr));
    }

    public WhereModifier Greatest(params object[] values)
    {
        var expr = Value.Greatest(values);
        return new WhereModifier(new ColumnModifier(Query, expr));
    }

    public WhereModifier Greatest(IEnumerable<object> values)
    {
        var expr = Value.Greatest(values);
        return new WhereModifier(new ColumnModifier(Query, expr));
    }

    public WhereModifier Least(params object[] values)
    {
        var expr = Value.Least(values);
        return new WhereModifier(new ColumnModifier(Query, expr));
    }

    public WhereModifier Least(IEnumerable<object> values)
    {
        var expr = Value.Least(values);
        return new WhereModifier(new ColumnModifier(Query, expr));
    }
}
