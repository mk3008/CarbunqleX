﻿using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;
using System.Globalization;

namespace Carbunqlex;

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
        return In(left, scalarSubQuery);
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
        return NotIn(left, scalarSubQuery);
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
        return new BinaryExpression("=", left, new FunctionExpression("any", string.Empty, arguments));
    }

    public static BinaryExpression Any(IValueExpression left, params object[] values)
    {
        return new BinaryExpression("=", left, new FunctionExpression("any", string.Empty, Array(values)));
    }

    public static BinaryExpression Any(IValueExpression left, ISelectQuery scalarSubQuery)
    {
        return new BinaryExpression("=", left, new FunctionExpression("any", string.Empty, scalarSubQuery));
    }

    public static FunctionExpression Function(string functionName, IEnumerable<IValueExpression> arguments, OverClause? overClause = null)
    {
        return new FunctionExpression(functionName, string.Empty, new ValueArguments(arguments), overClause);
    }

    public static FunctionExpression Function(string functionName, params IValueExpression[] arguments)
    {
        return new FunctionExpression(functionName, string.Empty, new ValueArguments(arguments));
    }

    public static ValueArguments ConstantSet(params object[] values)
    {
        var expressions = values.ToList().Select(static v => v is IValueExpression expr ? expr : Constant(v));
        return new ValueArguments(expressions);
    }

    public static ArrayExpression Array(params object[] values)
    {
        var expressions = values.ToList().Select(static v => v is IValueExpression expr ? expr : Constant(v));
        return new ArrayExpression(new ValueArguments(expressions));
    }

    public static FunctionExpression Greatest(IEnumerable<object> values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("greatest", string.Empty, new ValueArguments(expressions));
    }

    public static FunctionExpression Greatest(params object[] values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("greatest", string.Empty, new ValueArguments(expressions));
    }

    public static FunctionExpression Least(IEnumerable<object> values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("least", string.Empty, new ValueArguments(expressions));
    }

    public static FunctionExpression Least(params object[] values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("least", string.Empty, new ValueArguments(expressions));
    }

    public static FunctionExpression Coalesce(IEnumerable<object> values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("coalesce", string.Empty, new ValueArguments(expressions));
    }

    public static FunctionExpression Coalesce(params object[] values)
    {
        var expressions = values.Select(v => v is IValueExpression expr ? expr : Constant(v));
        return new FunctionExpression("coalesce", string.Empty, new ValueArguments(expressions));
    }

    public static IValueExpression Keyword(string v)
    {
        return new ConstantExpression(v);
    }
}
