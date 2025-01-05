namespace Carbunqlex.ValueExpressions;

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
        Equal(left, ValueBuilder.Constant(right));

    public static BinaryExpression NotEqual(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("<>", left, right);

    public static BinaryExpression NotEqual(this IValueExpression left, object right) =>
        NotEqual(left, ValueBuilder.Constant(right));

    public static BinaryExpression GreaterThan(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression(">", left, right);

    public static BinaryExpression GreaterThan(this IValueExpression left, object right) =>
        GreaterThan(left, ValueBuilder.Constant(right));

    public static BinaryExpression GreaterThanOrEqual(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression(">=", left, right);

    public static BinaryExpression GreaterThanOrEqual(this IValueExpression left, object right) =>
        GreaterThanOrEqual(left, ValueBuilder.Constant(right));

    public static BinaryExpression LessThan(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("<", left, right);

    public static BinaryExpression LessThan(this IValueExpression left, object right) =>
        LessThan(left, ValueBuilder.Constant(right));

    public static BinaryExpression LessThanOrEqual(this IValueExpression left, IValueExpression right) =>
        new BinaryExpression("<=", left, right);

    public static BinaryExpression LessThanOrEqual(this IValueExpression left, object right) =>
        LessThanOrEqual(left, ValueBuilder.Constant(right));

    public static LikeExpression Like(this IValueExpression left, IValueExpression right) =>
        ValueBuilder.Like(left, right);

    public static LikeExpression Like(this IValueExpression left, object right) =>
        Like(left, ValueBuilder.Constant(right));

    public static LikeExpression NotLike(this IValueExpression left, IValueExpression right) =>
        ValueBuilder.NotLike(left, right);

    public static LikeExpression NotLike(this IValueExpression left, object right) =>
        NotLike(left, ValueBuilder.Constant(right));

    public static InExpression In(this IValueExpression left, IArgumentExpression right) =>
        ValueBuilder.In(left, right);

    public static InExpression In(this IValueExpression left, params object[] values) =>
        ValueBuilder.In(left, ValueBuilder.ConstantSet(values));

    public static InExpression In(this IValueExpression left, ISelectQuery right) =>
        ValueBuilder.In(left, new ScalarSubquery(right));

    public static InExpression NotIn(this IValueExpression left, IArgumentExpression right) =>
        ValueBuilder.NotIn(left, right);

    public static InExpression NotIn(this IValueExpression left, params object[] values) =>
        ValueBuilder.NotIn(left, ValueBuilder.ConstantSet(values));

    public static InExpression NotIn(this IValueExpression left, ISelectQuery right) =>
        ValueBuilder.NotIn(left, new ScalarSubquery(right));

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
        ValueBuilder.Any(left, new ScalarSubquery(right));

    public static BinaryExpression Any(this IValueExpression left, IArgumentExpression right) =>
        ValueBuilder.Any(left, right);

    public static ParenthesizedExpression Parenthesize(this IValueExpression expression) =>
        new ParenthesizedExpression(expression);

    public static BetweenExpression Between(this IValueExpression expression, IValueExpression lower, IValueExpression upper) =>
        new BetweenExpression(false, expression, lower, upper);

    public static BetweenExpression Between(this IValueExpression expression, object lower, object upper) =>
        Between(expression, ValueBuilder.Constant(lower), ValueBuilder.Constant(upper));

    public static BetweenExpression NotBetween(this IValueExpression expression, IValueExpression lower, IValueExpression upper) =>
        new BetweenExpression(true, expression, lower, upper);

    public static BetweenExpression NotBetween(this IValueExpression expression, object lower, object upper) =>
        NotBetween(expression, ValueBuilder.Constant(lower), ValueBuilder.Constant(upper));

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
        Greatest(left, new ValueSet(values));

    public static FunctionExpression Least(this IValueExpression left, params object[] values)
    {
        return ValueBuilder.Least(new object[] { left }.Union(values));
    }

    public static FunctionExpression Least(this IValueExpression left, params IValueExpression[] values) =>
        Least(left, new ValueSet(values));
}
