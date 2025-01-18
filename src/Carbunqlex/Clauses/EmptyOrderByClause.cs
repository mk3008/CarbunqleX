namespace Carbunqlex.Clauses;

public class EmptyOrderByClause : IOrderByClause
{
    public static readonly EmptyOrderByClause Instance = new EmptyOrderByClause();
    private EmptyOrderByClause() { }
    public string ToSqlWithoutCte() => string.Empty;
    public IEnumerable<Token> GenerateTokensWithoutCte() => new List<Token>();
    public IEnumerable<ISelectQuery> GetQueries() => new List<ISelectQuery>();
    public bool MightHaveQueries => false;
}
