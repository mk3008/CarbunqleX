namespace Carbunqlex.Clauses;

public class DistinctClause : IDistinctClause
{
    public bool IsDistinct { get; } = true;

    public string ToSqlWithoutCte()
    {
        return "distinct";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, "distinct");
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        // DistinctClause does not directly use CTEs, so return an empty list
        return Enumerable.Empty<CommonTableClause>();
    }
}
