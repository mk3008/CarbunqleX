using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class GroupByClause : ISqlComponent
{
    public List<IValueExpression> GroupByColumns { get; } = new();

    public string ToSql()
    {
        if (GroupByColumns.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("group by ");
        sb.Append(string.Join(", ", GroupByColumns.Select(c => c.ToSql())));
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        if (GroupByColumns.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause, "group by", "group by")
        };
        foreach (var column in GroupByColumns)
        {
            lexemes.AddRange(column.GetLexemes());
        }
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "group by"));
        return lexemes;
    }
}
