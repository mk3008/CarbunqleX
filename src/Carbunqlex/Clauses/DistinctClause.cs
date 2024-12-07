using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class DistinctClause : IDistinctClause
{
    public bool IsDistinct { get; } = true;

    public string ToSql()
    {
        return "distinct";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        yield return new Lexeme(LexType.Keyword, "distinct");
    }
}
