using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class PartitionByClause : IPartitionByClause
{
    public List<IValueExpression> PartitionByColumns { get; }

    public bool MightHaveQueries => PartitionByColumns.Any(c => c.MightHaveQueries);
    public PartitionByClause(params IValueExpression[] partitionByColumns)
    {
        PartitionByColumns = partitionByColumns.ToList();
    }
    public PartitionByClause(List<IValueExpression> partitionByColumns)
    {
        PartitionByColumns = partitionByColumns;
    }

    public string ToSqlWithoutCte()
    {
        if (PartitionByColumns.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("partition by ");
        sb.Append(string.Join(", ", PartitionByColumns.Select(c => c.ToSqlWithoutCte())));
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (PartitionByColumns.Count == 0)
        {
            return Enumerable.Empty<Token>();
        }

        // Estimate the initial capacity for the tokens list.
        // Each column can return up to 1 lexeme, so we add a buffer.
        // For example, a column like "a.value" can return up to 1 lexeme:
        // "a.value"
        // Additionally, we add space for commas and the "partition by" keyword.
        int initialCapacity = PartitionByColumns.Count * 2 + 2;
        var tokens = new List<Token>(initialCapacity)
            {
                new Token(TokenType.StartClause, "partition by", "partition by")
            };

        foreach (var column in PartitionByColumns)
        {
            tokens.AddRange(column.GenerateTokensWithoutCte());
            tokens.Add(new Token(TokenType.Comma, ",", "partition by"));
        }

        if (tokens.Count > 1)
        {
            // Remove the last comma
            tokens.RemoveAt(tokens.Count - 1);
        }

        tokens.Add(new Token(TokenType.EndClause, string.Empty, "partition by"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return PartitionByColumns
            .Where(c => c.MightHaveQueries)
            .SelectMany(c => c.GetQueries());
    }
}
