using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using System.Globalization;

namespace Carbunqlex;

public class ColumnModifier(ISelectQuery query, IValueExpression expression, SelectExpression? selectExpression = null)
{
    public ISelectQuery Query { get; } = query;

    public IValueExpression Value { get; internal set; } = expression;

    private SelectExpression? _selectExpression = selectExpression;
    public SelectExpression? SelectExpression => _selectExpression ??= Query.GetSelectExpressions().Where(e => e.Value == Value).FirstOrDefault();

    private SelectModifier? _selectModifier;
    public SelectModifier SelectModifier => _selectModifier ??= new(this);

    private WhereModifier? _whereModifier;
    public WhereModifier WhereModifier => _whereModifier ??= new(this);

    public override string ToString()
    {
        try
        {
            return $"{Value.ToSqlWithoutCte()} : {Query.ToSqlWithoutCte()}";
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
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

    internal void AddCondition(string operatorSymbol, IValueExpression rightValue)
    {
        var condition = new BinaryExpression(operatorSymbol, Value, rightValue);
        AddCondition(condition);
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    public SelectModifier AddColumn(SelectExpression expr)
    {
        Query.AddColumn(expr);
        return new SelectModifier(new ColumnModifier(Query, expr.Value, expr));
    }

    public SelectModifier AddColumn(IValueExpression value, string alias)
    {
        var expr = new SelectExpression(value, alias);
        Query.AddColumn(expr);
        return new SelectModifier(new ColumnModifier(Query, expr.Value, expr));
    }

    internal IValueExpression ToValueExpression(object value)
    {
        return value is IValueExpression expr
            ? expr
            : ValueBuilder.Constant(value);
    }
}

public class SelectModifier
{
    private readonly ColumnModifier _queryAccessor;

    public SelectModifier(ColumnModifier queryAccessor)
    {
        _queryAccessor = queryAccessor;
    }

    public SelectModifier Greatest(params object[] values)
    {
        if (_queryAccessor.SelectExpression != null)
        {
            var expr = ValueBuilder.Greatest(new object[] { _queryAccessor.Value }.Union(values));
            _queryAccessor.SelectExpression.Value = expr;
            _queryAccessor.Value = expr;
        }
        return this;
    }

    public SelectModifier Greatest(IEnumerable<object> values)
    {
        if (_queryAccessor.SelectExpression != null)
        {
            var expr = ValueBuilder.Greatest(new object[] { _queryAccessor.Value }.Union(values));
            _queryAccessor.SelectExpression.Value = expr;
            _queryAccessor.Value = expr;
        }
        return this;
    }

    public SelectModifier Least(params object[] values)
    {
        if (_queryAccessor.SelectExpression != null)
        {
            var expr = ValueBuilder.Least(new object[] { _queryAccessor.Value }.Union(values));
            _queryAccessor.SelectExpression.Value = expr;
            _queryAccessor.Value = expr;
        }
        return this;
    }

    public SelectModifier Least(IEnumerable<object> values)
    {
        if (_queryAccessor.SelectExpression != null)
        {
            var expr = ValueBuilder.Least(new object[] { _queryAccessor.Value }.Union(values));
            _queryAccessor.SelectExpression.Value = expr;
            _queryAccessor.Value = expr;
        }
        return this;
    }

    public SelectModifier Coalesce(params object[] values)
    {
        if (_queryAccessor.SelectExpression != null)
        {
            var expr = ValueBuilder.Coalesce(new object[] { _queryAccessor.Value }.Union(values));
            _queryAccessor.SelectExpression.Value = expr;
            _queryAccessor.Value = expr;
        }
        return this;
    }

    public SelectModifier Coalesce(IEnumerable<object> values)
    {
        if (_queryAccessor.SelectExpression != null)
        {
            var expr = ValueBuilder.Coalesce(new object[] { _queryAccessor.Value }.Union(values));
            _queryAccessor.SelectExpression.Value = expr;
            _queryAccessor.Value = expr;
        }
        return this;
    }

    public void Remove()
    {
        if (_queryAccessor.SelectExpression != null)
        {
            _queryAccessor.Query.RemoveColumn(_queryAccessor.SelectExpression);
        }
    }
}

public class WhereModifier
{
    private readonly ColumnModifier _queryAccessor;

    public WhereModifier(ColumnModifier queryAccessor)
    {
        _queryAccessor = queryAccessor;
    }

    public WhereModifier Equal(object rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        _queryAccessor.AddCondition("=", rightExpression);
        return this;
    }

    public WhereModifier NotEqual(object rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        _queryAccessor.AddCondition("<>", rightExpression);
        return this;
    }

    public WhereModifier GreaterThan(object rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        _queryAccessor.AddCondition(">", rightExpression);
        return this;
    }

    public WhereModifier GreaterThanOrEqual(object rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        _queryAccessor.AddCondition(">=", rightExpression);
        return this;
    }

    public WhereModifier LessThan(object rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        _queryAccessor.AddCondition("<", rightExpression);
        return this;
    }

    public WhereModifier LessThanOrEqual(object rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        _queryAccessor.AddCondition("<=", rightExpression);
        return this;
    }

    public WhereModifier Like(object rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        var condition = ValueBuilder.Like(_queryAccessor.Value, rightExpression);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier Like(IArgumentExpression rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        var condition = ValueBuilder.Like(_queryAccessor.Value, rightExpression);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier NotLike(object rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        var condition = ValueBuilder.NotLike(_queryAccessor.Value, rightExpression);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier NotLike(IArgumentExpression rightValue)
    {
        var rightExpression = _queryAccessor.ToValueExpression(rightValue);
        var condition = ValueBuilder.NotLike(_queryAccessor.Value, rightExpression);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier In(params object[] values)
    {
        var condition = ValueBuilder.In(_queryAccessor.Value, values);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier In(ISelectQuery scalarSubQuery)
    {
        var condition = ValueBuilder.In(_queryAccessor.Value, scalarSubQuery);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier In(IArgumentExpression rightValue)
    {
        var condition = ValueBuilder.In(_queryAccessor.Value, rightValue);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier NotIn(params object[] values)
    {
        var condition = ValueBuilder.NotIn(_queryAccessor.Value, values);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier NotIn(ISelectQuery scalarSubQuery)
    {
        var condition = ValueBuilder.NotIn(_queryAccessor.Value, scalarSubQuery);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier NotIn(IArgumentExpression rightValue)
    {
        var condition = ValueBuilder.NotIn(_queryAccessor.Value, rightValue);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier Any(params object[] values)
    {
        var condition = ValueBuilder.Any(_queryAccessor.Value, values);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier Any(ISelectQuery scalarSubQuery)
    {
        var condition = ValueBuilder.Any(_queryAccessor.Value, scalarSubQuery);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier Any(IArgumentExpression rightValue)
    {
        var condition = ValueBuilder.Any(_queryAccessor.Value, rightValue);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier IsNull()
    {
        _queryAccessor.AddCondition("is", ValueBuilder.Null);
        return this;
    }

    public WhereModifier IsNotNull()
    {
        _queryAccessor.AddCondition("is", ValueBuilder.NotNull);
        return this;
    }

    public WhereModifier Between(object start, object end)
    {
        var startExpression = _queryAccessor.ToValueExpression(start);
        var endExpression = _queryAccessor.ToValueExpression(end);
        var condition = ValueBuilder.Between(_queryAccessor.Value, startExpression, endExpression);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier NotBetween(object start, object end)
    {
        var startExpression = _queryAccessor.ToValueExpression(start);
        var endExpression = _queryAccessor.ToValueExpression(end);
        var condition = ValueBuilder.NotBetween(_queryAccessor.Value, startExpression, endExpression);
        _queryAccessor.AddCondition(condition);
        return this;
    }

    public WhereModifier Coalesce(params object[] values)
    {
        var expr = ValueBuilder.Coalesce(new object[] { _queryAccessor.Value }.Union(values));
        return new WhereModifier(new ColumnModifier(_queryAccessor.Query, expr));
    }

    public WhereModifier Coalesce(IEnumerable<object> values)
    {
        var expr = ValueBuilder.Coalesce(new object[] { _queryAccessor.Value }.Union(values));
        return new WhereModifier(new ColumnModifier(_queryAccessor.Query, expr));
    }

    public WhereModifier Greatest(params object[] values)
    {
        var expr = ValueBuilder.Greatest(new object[] { _queryAccessor.Value }.Union(values));
        return new WhereModifier(new ColumnModifier(_queryAccessor.Query, expr));
    }

    public WhereModifier Greatest(IEnumerable<object> values)
    {
        var expr = ValueBuilder.Greatest(new object[] { _queryAccessor.Value }.Union(values));
        return new WhereModifier(new ColumnModifier(_queryAccessor.Query, expr));
    }

    public WhereModifier Least(params object[] values)
    {
        var expr = ValueBuilder.Least(new object[] { _queryAccessor.Value }.Union(values));
        return new WhereModifier(new ColumnModifier(_queryAccessor.Query, expr));
    }

    public WhereModifier Least(IEnumerable<object> values)
    {
        var expr = ValueBuilder.Least(new object[] { _queryAccessor.Value }.Union(values));
        return new WhereModifier(new ColumnModifier(_queryAccessor.Query, expr));
    }
}

public static class ValueBuilder
{
    public static NullExpression Null = new NullExpression(false);

    public static NullExpression NotNull = new NullExpression(true);

    public static ConstantExpression Constant(object value)
    {
        string columnValue;
        if (value is DateTime dateTimeValue)
        {
            columnValue = "'" + dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss") + "'";
        }
        else if (value is double doubleValue)
        {
            columnValue = doubleValue.ToString("G", CultureInfo.InvariantCulture);
        }
        else if (value is string stringValue)
        {
            columnValue = "'" + stringValue.Replace("'", "''") + "'";
        }
        else
        {
            columnValue = value?.ToString() ?? "null";
        }
        return new ConstantExpression(columnValue);
    }

    public static BetweenExpression Between(IValueExpression left, IValueExpression start, IValueExpression end)
    {
        return new BetweenExpression(false, left, start, end);
    }

    public static BetweenExpression NotBetween(IValueExpression left, IValueExpression start, IValueExpression end)
    {
        return new BetweenExpression(true, left, start, end);
    }

    public static InExpression In(IValueExpression left, IArgumentExpression right)
    {
        return new InExpression(false, left, right);
    }

    public static InExpression In(IValueExpression left, params object[] values)
    {
        return In(left, ConstantSet(values));
    }

    public static InExpression In(IValueExpression left, ISelectQuery scalarSubQuery)
    {
        return In(left, new ScalarSubquery(scalarSubQuery));
    }

    public static InExpression NotIn(IValueExpression left, IArgumentExpression right)
    {
        return new InExpression(true, left, right);
    }

    public static InExpression NotIn(IValueExpression left, params object[] values)
    {
        return NotIn(left, ConstantSet(values));
    }

    public static InExpression NotIn(IValueExpression left, ISelectQuery scalarSubQuery)
    {
        return NotIn(left, new ScalarSubquery(scalarSubQuery));
    }

    public static LikeExpression Like(IValueExpression left, IValueExpression right)
    {
        return new LikeExpression(false, left, right);
    }

    public static LikeExpression NotLike(IValueExpression left, IValueExpression right)
    {
        return new LikeExpression(true, left, right);
    }

    public static BinaryExpression Any(IValueExpression left, IArgumentExpression arguments)
    {
        return new BinaryExpression("=", left, new FunctionExpression("any", arguments));
    }

    public static BinaryExpression Any(IValueExpression left, params object[] values)
    {
        return new BinaryExpression("=", left, new FunctionExpression("any", Array(values)));
    }

    public static BinaryExpression Any(IValueExpression left, ISelectQuery scalarSubQuery)
    {
        return new BinaryExpression("=", left, new FunctionExpression("any", new ScalarSubquery(scalarSubQuery)));
    }

    public static FunctionExpression Function(string functionName, IEnumerable<IValueExpression> arguments, OverClause? overClause = null)
    {
        return new FunctionExpression(functionName, new ValueSet(arguments), overClause);
    }

    public static FunctionExpression Function(string functionName, params IValueExpression[] arguments)
    {
        return new FunctionExpression(functionName, new ValueSet(arguments));
    }

    public static ValueSet ConstantSet(params object[] values)
    {
        var expressions = values.ToList().Select(static v => v is IValueExpression expr ? expr : Constant(v));
        return new ValueSet(expressions);
    }

    public static ArrayExpression Array(params object[] values)
    {
        var expressions = values.ToList().Select(static v => v is IValueExpression expr ? expr : Constant(v));
        return new ArrayExpression(expressions);
    }

    public static FunctionExpression Greatest(IEnumerable<object> values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("greatest", new ValueSet(expressions));
    }

    public static FunctionExpression Greatest(params object[] values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("greatest", new ValueSet(expressions));
    }

    public static FunctionExpression Least(IEnumerable<object> values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("least", new ValueSet(expressions));
    }

    public static FunctionExpression Least(params object[] values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("least", new ValueSet(expressions));
    }

    public static FunctionExpression Coalesce(IEnumerable<object> values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("coalesce", new ValueSet(expressions));
    }

    public static FunctionExpression Coalesce(params object[] values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("coalesce", new ValueSet(expressions));
    }

    public static IValueExpression Keyword(string v)
    {
        return new ConstantExpression(v);
    }
}
