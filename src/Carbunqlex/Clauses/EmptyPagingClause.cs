namespace Carbunqlex.Clauses;

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

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Enumerable.Empty<Lexeme>();
    }

    public IEnumerable<IQuery> GetQueries()
    {
        return Enumerable.Empty<IQuery>();
    }
}
