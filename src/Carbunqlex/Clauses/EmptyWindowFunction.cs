namespace Carbunqlex.Clauses;

public class EmptyWindowFunction : IWindowFunction
{
    public static readonly EmptyWindowFunction Instance = new EmptyWindowFunction();
    private EmptyWindowFunction() { }

    public IPartitionByClause PartitionBy => EmptyPartitionByClause.Instance;
    public IOrderByClause OrderBy => EmptyOrderByClause.Instance;
    public IWindowFrame WindowFrame => EmptyWindowFrame.Instance;
    public bool MightHaveCommonTableClauses => false;

    public string ToSqlWithoutCte() => string.Empty;
    public IEnumerable<Token> GenerateTokensWithoutCte() => Enumerable.Empty<Token>();
    public IEnumerable<ISelectQuery> GetQueries() => new List<ISelectQuery>();
}
