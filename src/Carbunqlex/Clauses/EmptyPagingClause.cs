﻿namespace Carbunqlex.Clauses;

public class EmptyPagingClause : IPagingClause
{
    // Singleton instance
    public static readonly EmptyPagingClause Instance = new EmptyPagingClause();

    // Private constructor to prevent instantiation
    private EmptyPagingClause() { }

    public string ToSqlWithoutCte()
    {
        return string.Empty;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return Enumerable.Empty<Token>();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Enumerable.Empty<ISelectQuery>();
    }
}
