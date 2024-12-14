﻿namespace Carbunqlex.Clauses;

public class EmptyDistinctClause : IDistinctClause
{
    // Singleton instance
    public static readonly EmptyDistinctClause Instance = new EmptyDistinctClause();

    // Private constructor to prevent instantiation
    private EmptyDistinctClause() { }

    public string ToSqlWithoutCte()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Enumerable.Empty<Lexeme>();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        // EmptyDistinctClause does not directly use CTEs, so return an empty list
        return Enumerable.Empty<CommonTableClause>();
    }
}
