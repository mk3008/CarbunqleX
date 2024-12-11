using System.Text;

namespace Carbunqlex.Clauses;

public class OrderByClause : ISqlComponent
{
    public List<OrderByColumn> OrderByColumns { get; }

    public bool MightHaveCommonTableClauses => OrderByColumns.Any(column => column.Column.MightHaveCommonTableClauses);

    public OrderByClause(params OrderByColumn[] orderByColumns)
    {
        OrderByColumns = orderByColumns.ToList();
    }

    public string ToSqlWithoutCte()
    {
        if (OrderByColumns.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("order by ");
        sb.Append(string.Join(", ", OrderByColumns.Select(c => c.ToSqlWithoutCte())));
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        if (OrderByColumns.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        // Estimate the initial capacity for the lexemes list.
        // Each column can return multiple lexemes, so we add a buffer.
        // For example, a column like "a.value" can return up to 3 lexemes:
        // "a", ".", "value"
        // Additionally, we add space for commas and the "order by" keyword.
        int initialCapacity = OrderByColumns.Count * 4 + 1;
        var lexemes = new List<Lexeme>(initialCapacity)
            {
                new Lexeme(LexType.StartClause, "order by", "order by")
            };

        foreach (var orderByColumn in OrderByColumns)
        {
            lexemes.AddRange(orderByColumn.GenerateLexemesWithoutCte());
            lexemes.Add(new Lexeme(LexType.Comma, ",", "order by"));
        }

        if (lexemes.Count > 1)
        {
            // Remove the last comma
            lexemes.RemoveAt(lexemes.Count - 1);
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "order by"));
        return lexemes;
    }
    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return OrderByColumns
            .Where(orderByColumn => orderByColumn.Column.MightHaveCommonTableClauses)
            .SelectMany(orderByColumn => orderByColumn.Column.GetCommonTableClauses());
    }
}
