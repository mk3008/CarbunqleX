namespace Carbunqlex.Clauses;

public class EmptyWhereClause : IWhereClause
{
    // Singleton instance
    public static readonly EmptyWhereClause Instance = new EmptyWhereClause();

    // Private constructor to prevent instantiation
    private EmptyWhereClause() { }

    // ISqlComponent implementation
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

