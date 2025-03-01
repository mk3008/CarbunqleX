namespace Carbunqlex.Expressions;

public interface IValueExpression : ISqlComponent
{
    string DefaultName { get; }
    bool MightHaveQueries { get; }

    IEnumerable<ColumnExpression> ExtractColumnExpressions();
}

public static class IValueExpressionExtensions
{
    public static BinaryExpression Equal(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("=", left, right);

    public static BinaryExpression Equal(this IValueExpression left, object right) =>
        left.Equal(ValueBuilder.Constant(right));

    public static BinaryExpression NotEqual(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("<>", left, right);

    public static BinaryExpression NotEqual(this IValueExpression left, object right) =>
        left.NotEqual(ValueBuilder.Constant(right));

    public static BinaryExpression GreaterThan(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression(">", left, right);

    public static BinaryExpression GreaterThan(this IValueExpression left, object right) =>
        left.GreaterThan(ValueBuilder.Constant(right));

    public static BinaryExpression GreaterThanOrEqual(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression(">=", left, right);

    public static BinaryExpression GreaterThanOrEqual(this IValueExpression left, object right) =>
        left.GreaterThanOrEqual(ValueBuilder.Constant(right));

    public static BinaryExpression LessThan(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("<", left, right);

    public static BinaryExpression LessThan(this IValueExpression left, object right) =>
        left.LessThan(ValueBuilder.Constant(right));

    public static BinaryExpression LessThanOrEqual(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("<=", left, right);

    public static BinaryExpression LessThanOrEqual(this IValueExpression left, object right) =>
        left.LessThanOrEqual(ValueBuilder.Constant(right));

    public static LikeExpression Like(this IValueExpression left, IValueExpression right) =>
        ValueBuilder.Like(left, right);

    public static LikeExpression Like(this IValueExpression left, object right) =>
        left.Like(ValueBuilder.Constant(right));

    public static LikeExpression NotLike(this IValueExpression left, IValueExpression right) =>
        ValueBuilder.NotLike(left, right);

    public static LikeExpression NotLike(this IValueExpression left, object right) =>
        left.NotLike(ValueBuilder.Constant(right));

    public static InExpression In(this IValueExpression left, IValueGroupExpression right) =>
        ValueBuilder.In(new InValueGroupExpression(left), right);

    public static InExpression In(this IValueExpression left, IEnumerable<object> values) =>
        ValueBuilder.In(new InValueGroupExpression(left), ValueBuilder.CreateInClauseValueExpression(values));

    public static InExpression In(this IValueExpression left, ISelectQuery right) =>
        ValueBuilder.In(new InValueGroupExpression(left), new SubQueryExpression(right));

    public static InExpression NotIn(this IValueExpression left, IValueGroupExpression right) =>
        ValueBuilder.NotIn(new InValueGroupExpression(left), right);

    public static InExpression NotIn(this IValueExpression left, IEnumerable<object> values) =>
        ValueBuilder.NotIn(new InValueGroupExpression(left), ValueBuilder.CreateInClauseValueExpression(values));

    public static InExpression NotIn(this IValueExpression left, ISelectQuery right) =>
        ValueBuilder.NotIn(new InValueGroupExpression(left), new SubQueryExpression(right));

    public static BinaryExpression IsNull(this IValueExpression expression) =>
        new BinaryExpression("is", expression, ValueBuilder.Null);

    public static BinaryExpression IsNotNull(this IValueExpression expression) =>
        new BinaryExpression("is not", expression, ValueBuilder.Null);

    public static BinaryExpression And(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("and", left, right);

    public static BinaryExpression Or(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("or", left, right);

    public static BinaryExpression Any(this IValueExpression left, IValueExpression right) =>
       ValueBuilder.Any(left, right);

    public static BinaryExpression Any(this IValueExpression left, object[] right) =>
        ValueBuilder.Any(left, ValueBuilder.Array(right));

    public static BinaryExpression Any(this IValueExpression left, ISelectQuery right) =>
        ValueBuilder.Any(left, right);

    public static BinaryExpression Any(this IValueExpression left, IArgumentExpression right) =>
        ValueBuilder.Any(left, right);

    public static ParenthesizedExpression Parenthesize(this IValueExpression expression) =>
        new ParenthesizedExpression(expression);

    public static BetweenExpression Between(this IValueExpression expression, IValueExpression lower, IValueExpression upper) =>
        new BetweenExpression(false, expression, lower, upper);

    public static BetweenExpression Between(this IValueExpression expression, object lower, object upper) =>
        expression.Between(ValueBuilder.Constant(lower), ValueBuilder.Constant(upper));

    public static BetweenExpression NotBetween(this IValueExpression expression, IValueExpression lower, IValueExpression upper) =>
        new BetweenExpression(true, expression, lower, upper);

    public static BetweenExpression NotBetween(this IValueExpression expression, object lower, object upper) =>
        expression.NotBetween(ValueBuilder.Constant(lower), ValueBuilder.Constant(upper));

    public static FunctionExpression Coalesce(this IValueExpression left, params object[] values)
    {
        return ValueBuilder.Coalesce(new object[] { left }.Union(values));
    }

    public static FunctionExpression Coalesce(this IValueExpression left, params IValueExpression[] values)
    {
        return ValueBuilder.Coalesce(new[] { left }.Union(values));
    }

    public static FunctionExpression Greatest(this IValueExpression left, params object[] values)
    {
        return ValueBuilder.Greatest(new object[] { left }.Union(values));
    }

    public static FunctionExpression Greatest(this IValueExpression left, params IValueExpression[] values) =>
        left.Greatest(new ArgumentExpression(values));

    public static FunctionExpression Least(this IValueExpression left, params object[] values)
    {
        return ValueBuilder.Least(new object[] { left }.Union(values));
    }

    public static FunctionExpression Least(this IValueExpression left, params IValueExpression[] values) =>
        left.Least(new ArgumentExpression(values));
}
