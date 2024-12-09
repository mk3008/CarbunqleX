namespace Carbunqlex.Clauses;

public class EmptyDistinctClause : IDistinctClause
{
    public string ToSqlWithoutCte()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Enumerable.Empty<Lexeme>();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        // EmptyDistinctClause does not directly use CTEs, so return an empty list
        return Enumerable.Empty<CommonTableClause>();
    }
}
