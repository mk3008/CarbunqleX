using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class DistinctOnClause : IDistinctClause
{
    public bool IsDistinct { get; } = true;
    public List<IValueExpression> DistinctOnColumns { get; }

    public DistinctOnClause(params IValueExpression[] distinctOnColumns)
    {
        DistinctOnColumns = distinctOnColumns.ToList();
    }

    public string ToSqlWithoutCte()
    {
        return $"distinct on ({string.Join(", ", DistinctOnColumns.Select(c => c.ToSqlWithoutCte()))})";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, "distinct");
        yield return new Lexeme(LexType.Keyword, "on");
        yield return new Lexeme(LexType.OpenParen, "(");

        foreach (var lexeme in DistinctOnColumns.SelectMany(c => c.GenerateLexemesWithoutCte()))
        {
            yield return lexeme;
        }

        yield return new Lexeme(LexType.CloseParen, ")");
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (!DistinctOnColumns.Any(c => c.MightHaveCommonTableClauses))
        {
            return Enumerable.Empty<CommonTableClause>();
        }

        var commonTableClauses = new List<CommonTableClause>();

        foreach (var column in DistinctOnColumns)
        {
            if (column.MightHaveCommonTableClauses)
            {
                commonTableClauses.AddRange(column.GetCommonTableClauses());
            }
        }

        return commonTableClauses;
    }
}
