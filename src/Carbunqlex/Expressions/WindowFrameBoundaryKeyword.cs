using Carbunqlex.Lexing;

namespace Carbunqlex.Expressions;

public class WindowFrameBoundaryKeyword : IWindowFrameBoundaryExpression
{
    public string BoundaryKeyword { get; }

    public WindowFrameBoundaryKeyword(string boundaryKeyword)
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
