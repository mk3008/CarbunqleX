using Carbunqlex.Clauses;

namespace Carbunqlex.ValueExpressions;

public class NullExpression : IValueExpression
{
    public bool IsNotNull { get; set; }
    public bool MightHaveCommonTableClauses => false;

    public NullExpression(bool isNotNull = false)
    {
        IsNotNull = isNotNull;
    }

    public string DefaultName => string.Empty;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, IsNotNull ? "not null" : "null");
    }

    public string ToSqlWithoutCte()
    {
        return IsNotNull ? "not null" : "null";
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        // NullExpression does not directly use CTEs, so return an empty list
        return Enumerable.Empty<CommonTableClause>();
    }
}
