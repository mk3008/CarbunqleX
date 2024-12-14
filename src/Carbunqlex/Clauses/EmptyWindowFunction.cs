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
    public IEnumerable<Lexeme> GenerateLexemesWithoutCte() => Enumerable.Empty<Lexeme>();
    public IEnumerable<CommonTableClause> GetCommonTableClauses() => Enumerable.Empty<CommonTableClause>();
}
