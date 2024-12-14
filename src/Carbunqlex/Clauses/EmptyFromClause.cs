namespace Carbunqlex.Clauses;

public class EmptyFromClause : IFromClause
{
    // Singleton instance
    public static readonly EmptyFromClause Instance = new EmptyFromClause();

    // Private constructor to prevent instantiation
    private EmptyFromClause() { }

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
        // EmptyFromClause does not directly use queries, so return an empty list
        return Enumerable.Empty<IQuery>();
    }
}
