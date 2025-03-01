using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Editors;

public class WhereEditor(ISelectQuery query, IReadOnlyDictionary<string, IValueExpression> keyValues)
{
    internal readonly IReadOnlyDictionary<string, IValueExpression> ValueMap = keyValues;

    internal readonly ISelectQuery Query = query;

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    private void AddCondition(IValueExpression condition)
    {
        if (Query.TryGetWhereClause(out var whereClause))
        {
            whereClause.Add(condition);
        }
        else
        {
            throw new NotSupportedException("The query must have a WHERE clause.");
        }
    }

    public WhereEditor Equal(object rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Equal(rightValue));
        }
        return this;
    }

    public WhereEditor NotEqual(object rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.NotEqual(rightValue));
        }
        return this;
    }

    public WhereEditor GreaterThan(object rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.GreaterThan(rightValue));
        }
        return this;
    }

    public WhereEditor GreaterThanOrEqual(object rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.GreaterThanOrEqual(rightValue));
        }
        return this;
    }

    public WhereEditor LessThan(object rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.LessThan(rightValue));
        }
        return this;
    }

    public WhereEditor LessThanOrEqual(object rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.LessThanOrEqual(rightValue));
        }
        return this;
    }

    public WhereEditor Like(IValueExpression rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Like(rightValue));
        }
        return this;
    }

    public WhereEditor Like(object rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Like(rightValue));
        }
        return this;
    }

    public WhereEditor NotLike(IValueExpression rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.NotLike(rightValue));
        }
        return this;
    }

    public WhereEditor NotLike(object rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.NotLike(rightValue));
        }
        return this;
    }

    public WhereEditor In(IValueGroupExpression rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.In(rightValue));
        }
        return this;
    }

    public WhereEditor In(IEnumerable<object> values)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.In(values));
        }
        return this;
    }

    public WhereEditor In(IQuery subQuery)
    {
        if (subQuery.TryGetSelectQuery(out var xsq))
        {
            var expressions = new List<SelectExpression>();
            foreach (var (key, value) in ValueMap)
            {
                expressions.Add(new SelectExpression(new ColumnExpression("x", value.DefaultName ?? key)));
            }

            var sq = new SelectQuery(
                new SelectClause(expressions),
                new FromClause(new DatasourceExpression(new SubQuerySource(xsq), "x"))
                );

            AddCondition(new InExpression(false,
                new InValueGroupExpression(ValueMap.Values.ToList()),
                new SubQueryExpression(sq)));
        }
        else
        {
            throw new InvalidOperationException("The subquery must be a SELECT query.");
        }
        return this;
    }

    public WhereEditor Exists(IQuery subQuery)
    {
        if (subQuery.TryGetSelectQuery(out var selectQuery))
        {
            var sq = new SelectQuery(
                new SelectClause(new SelectExpression(new ColumnExpression("*"))),
                new FromClause(new DatasourceExpression(new SubQuerySource(selectQuery), "x"))
                );
            foreach (var (key, value) in ValueMap)
            {
                sq.WhereClause.Add(value.Equal(new ColumnExpression("x", value.DefaultName ?? key)));
            }
            AddCondition(new ExistsExpression(false, sq));
        }
        else
        {
            throw new NotSupportedException("The subquery must be a SELECT query.");
        }
        return this;
    }

    public WhereEditor NotIn(IEnumerable<object> values)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.NotIn(values));
        }
        return this;
    }

    public WhereEditor NotIn(IValueGroupExpression rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.NotIn(rightValue));
        }
        return this;
    }

    public WhereEditor NotIn(IQuery subQuery)
    {
        if (subQuery.TryGetSelectQuery(out var xsq))
        {
            var expressions = new List<SelectExpression>();
            foreach (var (key, value) in ValueMap)
            {
                expressions.Add(new SelectExpression(new ColumnExpression("x", value.DefaultName ?? key)));
            }

            var sq = new SelectQuery(
                new SelectClause(expressions),
                new FromClause(new DatasourceExpression(new SubQuerySource(xsq), "x"))
                );

            AddCondition(new InExpression(true,
                new InValueGroupExpression(ValueMap.Values.ToList()),
                new SubQueryExpression(sq)));
        }
        else
        {
            throw new NotSupportedException("The subquery must be a SELECT query.");
        }
        return this;
    }

    public WhereEditor NotExists(IQuery subQuery)
    {
        if (subQuery.TryGetSelectQuery(out var selectQuery))
        {
            var sq = new SelectQuery(
                new SelectClause(new SelectExpression(new ColumnExpression("*"))),
                new FromClause(new DatasourceExpression(new SubQuerySource(selectQuery), "x"))
                );
            foreach (var (key, value) in ValueMap)
            {
                sq.WhereClause.Add(value.Equal(new ColumnExpression("x", value.DefaultName ?? key)));
            }
            AddCondition(new ExistsExpression(true, sq));
        }
        else
        {
            throw new NotSupportedException("The subquery must be a SELECT query.");
        }
        return this;
    }

    public WhereEditor Any(params object[] values)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Any(values));
        }
        return this;
    }

    public WhereEditor Any(ISelectQuery scalarSubQuery)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Any(scalarSubQuery));
        }
        return this;
    }

    public WhereEditor Any(IArgumentExpression rightValue)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Any(rightValue));
        }
        return this;
    }


    public WhereEditor IsNull()
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.IsNull());
        }
        return this;
    }

    public WhereEditor IsNotNull()
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.IsNotNull());
        }
        return this;
    }

    public WhereEditor Between(object start, object end)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Between(start, end));
        }
        return this;
    }

    public WhereEditor NotBetween(object start, object end)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.NotBetween(start, end));
        }
        return this;
    }

    public WhereEditor Coalesce(object nullValue, Action<WhereEditor> action)
    {
        foreach (var (key, value) in ValueMap)
        {
            var expr = value.Coalesce(nullValue);
            var editor = new WhereEditor(Query, new Dictionary<string, IValueExpression> { { string.Empty, expr } });
            action(editor);
        }
        return this;
    }

    public WhereEditor Greatest(params object[] values)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Greatest(values));
        }
        return this;
    }

    public WhereEditor Greatest(IEnumerable<object> values)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Greatest(values));
        }
        return this;
    }

    public WhereEditor Least(params object[] values)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Least(values));
        }
        return this;
    }

    public WhereEditor Least(IEnumerable<object> values)
    {
        foreach (var (key, value) in ValueMap)
        {
            AddCondition(value.Least(values));
        }
        return this;
    }
}
