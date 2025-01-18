namespace Carbunqlex.Clauses;

public class EmptyWindowFrame : IWindowFrame
{
    public static readonly EmptyWindowFrame Instance = new EmptyWindowFrame();
    private EmptyWindowFrame() { }
    public string ToSqlWithoutCte() => string.Empty;
    public IEnumerable<Token> GenerateTokensWithoutCte() => new List<Token>();
    public IEnumerable<ISelectQuery> GetQueries() => new List<ISelectQuery>();
    public bool MightHaveCommonTableClauses => false;
}
