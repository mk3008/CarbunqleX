using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class WindowFrameBoundary : ISqlComponent
{
    public WindowFrameBoundaryExpression Boundary { get; }

    private WindowFrameBoundary(WindowFrameBoundaryExpression boundary)
    {
        Boundary = boundary;
    }

    public static WindowFrameBoundary UnboundedPreceding => new WindowFrameBoundary(WindowFrameBoundaryExpression.UnboundedPreceding);
    public static WindowFrameBoundary CurrentRow => new WindowFrameBoundary(WindowFrameBoundaryExpression.CurrentRow);
    public static WindowFrameBoundary UnboundedFollowing => new WindowFrameBoundary(WindowFrameBoundaryExpression.UnboundedFollowing);

    public static WindowFrameBoundary Preceding(IValueExpression rows) => new WindowFrameBoundary(WindowFrameBoundaryExpression.Preceding(rows));
    public static WindowFrameBoundary Following(IValueExpression rows) => new WindowFrameBoundary(WindowFrameBoundaryExpression.Following(rows));
    public static WindowFrameBoundary Preceding(int rows) => new WindowFrameBoundary(WindowFrameBoundaryExpression.Preceding(new ConstantExpression(rows)));
    public static WindowFrameBoundary Following(int rows) => new WindowFrameBoundary(WindowFrameBoundaryExpression.Following(new ConstantExpression(rows)));

    public string ToSql()
    {
        return Boundary.ToSql();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        return Boundary.GetLexemes();
    }
}
