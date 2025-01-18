namespace Carbunqlex.Clauses;

internal class EmptyColumnAliasClause : IColumnAliasClause
{
    // Singleton instance
    public static readonly IColumnAliasClause Instance = new EmptyColumnAliasClause();

    // Private constructor to prevent instantiation
    private EmptyColumnAliasClause() { }

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

    public IEnumerable<string> GetColumnNames()
    {
        return Enumerable.Empty<string>();
    }
}
