using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class PartitionByClause : ISqlComponent
{
    public List<IValueExpression> PartitionByColumns { get; } = new();

    public string ToSql()
    {
        if (PartitionByColumns.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("partition by ");
        sb.Append(string.Join(", ", PartitionByColumns.Select(c => c.ToSql())));
        return sb.ToString();
    }
    public IEnumerable<Lexeme> GetLexemes()
    {
        if (PartitionByColumns.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause, "partition by", "partition by")
        };
        foreach (var column in PartitionByColumns)
        {
            lexemes.AddRange(column.GetLexemes());
        }
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "partition by"));
        return lexemes;
    }
}
