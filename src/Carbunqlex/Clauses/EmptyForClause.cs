﻿namespace Carbunqlex.Clauses;

public class EmptyForClause : IForClause
{
    // Singleton instance
    public static readonly EmptyForClause Instance = new EmptyForClause();

    // Private constructor to prevent instantiation
    private EmptyForClause() { }

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
        // EmptyForClause does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }
}
