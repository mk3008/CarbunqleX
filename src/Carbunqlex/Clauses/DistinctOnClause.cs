using Carbunqlex.ValueExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Carbunqlex.Clauses;

public class DistinctOnClause : IDistinctClause
{
    public bool IsDistinct { get; } = true;
    public List<IValueExpression> DistinctOnColumns { get; }

    public DistinctOnClause(params IValueExpression[] distinctOnColumns)
    {
        DistinctOnColumns = distinctOnColumns.ToList();
    }

    public string ToSql()
    {
        return $"distinct on ({string.Join(", ", DistinctOnColumns.Select(c => c.ToSql()))})";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
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
