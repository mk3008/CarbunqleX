using System.Collections.Generic;

namespace Carbunqlex.Clauses;

public class EmptyWhereClause : IWhereClause
{
    // Singleton instance
    public static readonly EmptyWhereClause Instance = new EmptyWhereClause();

    // Private constructor to prevent instantiation
    private EmptyWhereClause() { }

    public string ToSql()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        return Enumerable.Empty<Lexeme>();
    }
}

