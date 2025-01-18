using System.Text;

namespace Carbunqlex.Clauses;

public class OrderByClause : IOrderByClause
{
    public List<OrderByColumn> OrderByColumns { get; }

    public bool MightHaveQueries => OrderByColumns.Any(column => column.Column.MightHaveQueries);

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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (OrderByColumns.Count == 0)
        {
            return Enumerable.Empty<Token>();
        }

        // Estimate the initial capacity for the tokens list.
        // Each column can return multiple tokens, so we add a buffer.
        // For example, a column like "a.value" can return up to 3 tokens:
        // "a", ".", "value"
        // Additionally, we add space for commas and the "order by" keyword.
        int initialCapacity = OrderByColumns.Count * 4 + 1;
        var tokens = new List<Token>(initialCapacity)
        {
            new Token(TokenType.StartClause, "order by", "order by")
        };

        foreach (var orderByColumn in OrderByColumns)
        {
            tokens.AddRange(orderByColumn.GenerateTokensWithoutCte());
            tokens.Add(new Token(TokenType.Comma, ",", "order by"));
        }

        if (tokens.Count > 1)
        {
            // Remove the last comma
            tokens.RemoveAt(tokens.Count - 1);
        }

        tokens.Add(new Token(TokenType.EndClause, string.Empty, "order by"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return OrderByColumns
            .Where(orderByColumn => orderByColumn.Column.MightHaveQueries)
            .SelectMany(orderByColumn => orderByColumn.Column.GetQueries());
    }
}
