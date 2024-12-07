namespace Carbunqlex.Clauses;

public class EmptyForClause : IForClause
{
    // Singleton instance
    public static readonly EmptyForClause Instance = new EmptyForClause();

    // Private constructor to prevent instantiation
    private EmptyForClause() { }

    public string ToSql()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        return Enumerable.Empty<Lexeme>();
    }
}
