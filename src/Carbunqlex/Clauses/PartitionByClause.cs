﻿using Carbunqlex.ValueExpressions;
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

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        if (PartitionByColumns.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        // Estimate the initial capacity for the lexemes list.
        // Each column can return up to 1 lexeme, so we add a buffer.
        // For example, a column like "a.value" can return up to 1 lexeme:
        // "a.value"
        // Additionally, we add space for commas and the "partition by" keyword.
        int initialCapacity = PartitionByColumns.Count * 2 + 2;
        var lexemes = new List<Lexeme>(initialCapacity)
            {
                new Lexeme(LexType.StartClause, "partition by", "partition by")
            };

        foreach (var column in PartitionByColumns)
        {
            lexemes.AddRange(column.GenerateLexemesWithoutCte());
            lexemes.Add(new Lexeme(LexType.Comma, ",", "partition by"));
        }

        if (lexemes.Count > 1)
        {
            // Remove the last comma
            lexemes.RemoveAt(lexemes.Count - 1);
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "partition by"));
        return lexemes;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return PartitionByColumns
            .Where(c => c.MightHaveQueries)
            .SelectMany(c => c.GetQueries());
    }
}
