namespace Carbunqlex.Clauses;

public class WindowFrameBoundary : ISqlComponent
{
    public string Boundary { get; }
    public string DefaultName => string.Empty;

    private WindowFrameBoundary(string boundary)
    {
        Boundary = boundary;
    }

    public static WindowFrameBoundary UnboundedPreceding => new WindowFrameBoundary("unbounded preceding");
    public static WindowFrameBoundary CurrentRow => new WindowFrameBoundary("current row");
    public static WindowFrameBoundary UnboundedFollowing => new WindowFrameBoundary("unbounded following");

    public static WindowFrameBoundary Preceding(int rows) => new WindowFrameBoundary($"{rows} preceding");
    public static WindowFrameBoundary Following(int rows) => new WindowFrameBoundary($"{rows} following");

    public string ToSql()
    {
        return Boundary;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        return new List<Lexeme> { new Lexeme(LexType.Keyword, Boundary) };
    }
}
