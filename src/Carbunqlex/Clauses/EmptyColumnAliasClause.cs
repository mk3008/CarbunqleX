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

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Enumerable.Empty<Lexeme>();
    }

    public IEnumerable<IQuery> GetQueries()
    {
        return Enumerable.Empty<IQuery>();
    }

    public IEnumerable<string> GetColumnNames()
    {
        return Enumerable.Empty<string>();
    }
}
