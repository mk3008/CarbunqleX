namespace Carbunqlex.Clauses;

public class EmptyWindowFrame : IWindowFrame
{
    public static readonly EmptyWindowFrame Instance = new EmptyWindowFrame();
    private EmptyWindowFrame() { }
    public string ToSqlWithoutCte() => string.Empty;
    public IEnumerable<Lexeme> GenerateLexemesWithoutCte() => new List<Lexeme>();
    public IEnumerable<CommonTableClause> GetCommonTableClauses() => new List<CommonTableClause>();
    public bool MightHaveCommonTableClauses => false;
}
