using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using System.Globalization;

namespace Carbunqlex;

public class QueryAccessor(ISelectQuery query, IValueExpression expression)
{
    public ISelectQuery Query { get; } = query;
    public IValueExpression LeftExpression { get; } = expression;

    public override string ToString()
    {
        try
        {
            return $"{LeftExpression.ToSqlWithoutCte()} : {Query.ToSqlWithoutCte()}";
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }

    private void AddCondition(IValueExpression condition)
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

    public void AddCondition(string operatorSymbol, IValueExpression rightValue)
    {
        var condition = new BinaryExpression(operatorSymbol, LeftExpression, rightValue);
        AddCondition(condition);
    }

    public QueryAccessor Equal(object rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        AddCondition("=", rightExpression);
        return this;
    }

    public QueryAccessor NotEqual(object rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        AddCondition("<>", rightExpression);
        return this;
    }

    public QueryAccessor GreaterThan(object rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        AddCondition(">", rightExpression);
        return this;
    }

    public QueryAccessor GreaterThanOrEqual(object rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        AddCondition(">=", rightExpression);
        return this;
    }

    public QueryAccessor LessThan(object rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        AddCondition("<", rightExpression);
        return this;
    }

    public QueryAccessor LessThanOrEqual(object rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        AddCondition("<=", rightExpression);
        return this;
    }

    public QueryAccessor Like(object rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        var condition = ValueBuilder.Like(LeftExpression, rightExpression);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor Like(IArgumentExpression rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        var condition = ValueBuilder.Like(LeftExpression, rightExpression);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor NotLike(object rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        var condition = ValueBuilder.NotLike(LeftExpression, rightExpression);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor NotLike(IArgumentExpression rightValue)
    {
        var rightExpression = ToValueExpression(rightValue);
        var condition = ValueBuilder.NotLike(LeftExpression, rightExpression);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor In(params object[] values)
    {
        var condition = ValueBuilder.In(LeftExpression, values);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor In(ISelectQuery scalarSubQuery)
    {
        var condition = ValueBuilder.In(LeftExpression, scalarSubQuery);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor In(IArgumentExpression rightValue)
    {
        var condition = ValueBuilder.In(LeftExpression, rightValue);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor NotIn(params object[] values)
    {
        var condition = ValueBuilder.NotIn(LeftExpression, values);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor NotIn(ISelectQuery scalarSubQuery)
    {
        var condition = ValueBuilder.NotIn(LeftExpression, scalarSubQuery);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor NotIn(IArgumentExpression rightValue)
    {
        var condition = ValueBuilder.NotIn(LeftExpression, rightValue);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor Any(params object[] values)
    {
        var condition = ValueBuilder.Any(LeftExpression, values);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor Any(ISelectQuery scalarSubQuery)
    {
        var condition = ValueBuilder.Any(LeftExpression, scalarSubQuery);
        AddCondition(condition);
        return this;
    }

    public QueryAccessor Any(IArgumentExpression rightValue)
    {
        var condition = ValueBuilder.Any(LeftExpression, rightValue);
        AddCondition(condition);
        return this;
    }

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    //private static ConstantExpressionSet CreateConstantExpressionSet(params object[] values)
    //{
    //    var expressions = values.ToList().Select(v => v is IValueExpression expr ? expr : ValueBuilder.BuildConstant(v));
    //    return new ConstantExpressionSet(expressions);
    //}

    //private static ConstantExpressionSet CreateConstantExpressionSet<T>(IEnumerable<T> values)
    //{
    //    var expressions = values.ToList().Select(v => v is IValueExpression expr ? expr : ExpressionBuilder.BuildConstantExpression(v));
    //    return new ConstantExpressionSet(expressions);
    //}

    //public QueryAccessor Any(ISelectQuery subQuery)
    //{
    //    var rightExpression = ToValueExpression(rightValue);
    //}

    public IValueExpression ToValueExpression(object value)
    {
        return value is IValueExpression expr
            ? expr
            : ValueBuilder.Constant(value);
    }
}

public static class ValueBuilder
{
    public static ConstantExpression Null = new ConstantExpression("null");

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
}
