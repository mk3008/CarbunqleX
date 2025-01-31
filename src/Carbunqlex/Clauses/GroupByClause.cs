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

    public GroupByClause(List<IValueExpression> groupByColumns)
    {
        GroupByColumns = groupByColumns;
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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (GroupByColumns.Count == 0)
        {
            return Enumerable.Empty<Token>();
        }

        // Estimate the initial capacity for the tokens list.
        // Each column can return multiple tokens, so we add a buffer.
        // For example, a column like "a.value" can return up to 4 tokens:
        // "a", ".", "value"
        // Additionally, we add space for commas and the "group by" keyword.
        int initialCapacity = GroupByColumns.Count * 5 + 1;
        var tokens = new List<Token>(initialCapacity)
        {
            new Token(TokenType.StartClause, "group by", "group by")
        };

        foreach (var column in GroupByColumns)
        {
            tokens.AddRange(column.GenerateTokensWithoutCte());
            tokens.Add(new Token(TokenType.Comma, ",", "group by"));
        }

        if (tokens.Count > 1)
        {
            // Remove the last comma
            tokens.RemoveAt(tokens.Count - 1);
        }

        tokens.Add(new Token(TokenType.EndClause, string.Empty, "group by"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return GroupByColumns
            .Where(column => column.MightHaveQueries)
            .SelectMany(column => column.GetQueries());
    }
}
