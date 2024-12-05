using System.Text;

namespace Carbunqlex.Clauses;

public class OrderByClause : ISqlComponent
{
    public List<OrderByColumn> OrderByColumns { get; } = new();

    public string ToSql()
    {
        if (OrderByColumns.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("order by ");
        sb.Append(string.Join(", ", OrderByColumns.Select(c => c.ToSql())));
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        if (OrderByColumns.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause, "order by", "order by")
        };
        foreach (var orderByColumn in OrderByColumns)
        {
            lexemes.AddRange(orderByColumn.GetLexemes());
        }
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "order by"));
        return lexemes;
    }
}
