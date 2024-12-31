using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class WindowFrameBoundary : ISqlComponent
{
    public IWindowFrameBoundaryExpression Boundary { get; }

    private WindowFrameBoundary(IWindowFrameBoundaryExpression boundary)
    {
        Boundary = boundary;
    }

    public bool MightHaveQueries => Boundary.MightHaveQueries;

    public static readonly WindowFrameBoundary UnboundedPreceding = new WindowFrameBoundary(FrameBoundaryKeyword.UnboundedPreceding);
    public static readonly WindowFrameBoundary CurrentRow = new WindowFrameBoundary(FrameBoundaryKeyword.CurrentRow);
    public static readonly WindowFrameBoundary UnboundedFollowing = new WindowFrameBoundary(FrameBoundaryKeyword.UnboundedFollowing);

    public static WindowFrameBoundary Preceding(IValueExpression rows) => new WindowFrameBoundary(FrameBoundaryExpression.Preceding(rows));
    public static WindowFrameBoundary Following(IValueExpression rows) => new WindowFrameBoundary(FrameBoundaryExpression.Following(rows));
    public static WindowFrameBoundary Preceding(int rows) => new WindowFrameBoundary(FrameBoundaryExpression.Preceding(new ConstantExpression(rows)));
    public static WindowFrameBoundary Following(int rows) => new WindowFrameBoundary(FrameBoundaryExpression.Following(new ConstantExpression(rows)));

    public string ToSqlWithoutCte()
    {
        return Boundary.ToSqlWithoutCte();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Boundary.GenerateLexemesWithoutCte();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Boundary.GetQueries();
    }
}
