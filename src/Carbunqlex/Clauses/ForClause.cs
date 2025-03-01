using Carbunqlex.Lexing;

namespace Carbunqlex.Clauses;

public class ForClause : IForClause
{
    public string LockType { get; }

    public ForClause(string lockType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockType, nameof(lockType));
        LockType = lockType;
    }

    public string ToSqlWithoutCte()
    {
        return $"for {LockType}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return new List<Token>
            {
                new Token(TokenType.StartClause, "for", "for"),
                new Token(TokenType.Command, LockType),
                new Token(TokenType.EndClause, string.Empty, "for")
            };
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // ForClause does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }
}
