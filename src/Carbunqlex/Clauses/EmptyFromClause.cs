namespace Carbunqlex.Clauses;

public class EmptyFromClause : IFromClause
{
    // Singleton instance
    public static readonly EmptyFromClause Instance = new EmptyFromClause();

    // Private constructor to prevent instantiation
    private EmptyFromClause() { }

    public string ToSql()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        return Enumerable.Empty<Lexeme>();
    }
}
