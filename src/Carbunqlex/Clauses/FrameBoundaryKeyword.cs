namespace Carbunqlex.Clauses;

public class FrameBoundaryKeyword : IWindowFrameBoundaryExpression
{
    public static readonly FrameBoundaryKeyword UnboundedPreceding = new FrameBoundaryKeyword("unbounded preceding");
    public static readonly FrameBoundaryKeyword CurrentRow = new FrameBoundaryKeyword("current row");
    public static readonly FrameBoundaryKeyword UnboundedFollowing = new FrameBoundaryKeyword("unbounded following");

    public string BoundaryKeyword { get; }

    private FrameBoundaryKeyword(string boundaryKeyword)
    {
        BoundaryKeyword = boundaryKeyword;
    }

    public bool MightHaveQueries => false;

    public string ToSqlWithoutCte()
    {
        return BoundaryKeyword;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return new List<Token> { new Token(TokenType.Command, BoundaryKeyword) };
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // FrameBoundaryKeyword does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }
}
