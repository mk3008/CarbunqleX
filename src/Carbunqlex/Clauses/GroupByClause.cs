using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class GroupByClause : ISqlComponent
{
    public List<IValueExpression> GroupByColumns { get; }

    public GroupByClause(params IValueExpression[] groupByColumns)
    {
        GroupByColumns = groupByColumns.ToList();
    }

    public string ToSqlWithoutCte()
    {
        if (GroupByColumns.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("group by ");
        sb.Append(string.Join(", ", GroupByColumns.Select(c => c.ToSqlWithoutCte())));
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        if (GroupByColumns.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        // Estimate the initial capacity for the lexemes list.
        // Each column can return multiple lexemes, so we add a buffer.
        // For example, a column like "a.value" can return up to 4 lexemes:
        // "a", ".", "value"
        // Additionally, we add space for commas and the "group by" keyword.
        int initialCapacity = GroupByColumns.Count * 5 + 1;
        var lexemes = new List<Lexeme>(initialCapacity)
        {
            new Lexeme(LexType.StartClause, "group by", "group by")
        };

        foreach (var column in GroupByColumns)
        {
            lexemes.AddRange(column.GenerateLexemesWithoutCte());
            lexemes.Add(new Lexeme(LexType.Comma, ",", "group by"));
        }

        if (lexemes.Count > 1)
        {
            // Remove the last comma
            lexemes.RemoveAt(lexemes.Count - 1);
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "group by"));
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return GroupByColumns
            .Where(column => column.MightHaveCommonTableClauses)
            .SelectMany(column => column.GetCommonTableClauses());
    }
}
