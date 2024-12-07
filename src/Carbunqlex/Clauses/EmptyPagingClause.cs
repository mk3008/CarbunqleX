using System.Collections.Generic;

namespace Carbunqlex.Clauses;

public class EmptyPagingClause : IPagingClause
{
    // Singleton instance
    public static readonly EmptyPagingClause Instance = new EmptyPagingClause();

    // Private constructor to prevent instantiation
    private EmptyPagingClause() { }

    public string ToSql()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        return Enumerable.Empty<Lexeme>();
    }
}
