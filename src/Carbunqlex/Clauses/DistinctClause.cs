using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class DistinctClause : ISqlComponent
{
    public bool IsDistinct { get; }
    public List<IValueExpression> DistinctOnColumns { get; }

    public DistinctClause(bool isDistinct)
    {
        IsDistinct = isDistinct;
        DistinctOnColumns = new();
    }

    public DistinctClause(params IValueExpression[] distinctOnColumns)
    {
        IsDistinct = true;
        DistinctOnColumns = distinctOnColumns.ToList();
    }

    public string ToSql()
    {
        if (!IsDistinct)
        {
            return string.Empty;
        }

        if (DistinctOnColumns.Count == 0)
        {
            return "distinct";
        }

        return $"distinct on ({string.Join(", ", DistinctOnColumns.Select(c => c.ToSql()))})";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        if (DistinctOnColumns.Count == 0)
        {
            yield return new Lexeme(LexType.Keyword, "distinct");
            yield break;
        }

        yield return new Lexeme(LexType.Keyword, "distinct");
        yield return new Lexeme(LexType.Keyword, "on");
        yield return new Lexeme(LexType.OpenParen, "(");

        foreach (var lexeme in DistinctOnColumns.SelectMany(c => c.GetLexemes()))
        {
            yield return lexeme;
        }

        yield return new Lexeme(LexType.CloseParen, ")");
    }
}
