namespace Carbunqlex.Clauses;

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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return Enumerable.Empty<Token>();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // EmptyDistinctClause does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }
}
