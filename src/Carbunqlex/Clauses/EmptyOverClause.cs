﻿namespace Carbunqlex.Clauses;

public class EmptyOverClause : IOverClause
{
    // Singleton instance
    public static readonly EmptyOverClause Instance = new EmptyOverClause();

    // Private constructor to prevent instantiation
    private EmptyOverClause() { }

    public bool MightHaveCommonTableClauses => false;

    public string ToSqlWithoutCte()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Enumerable.Empty<Lexeme>();
    }

    public IEnumerable<IQuery> GetQueries()
    {
        // EmptyOverClause does not directly use queries, so return an empty list
        return Enumerable.Empty<IQuery>();
    }
}
