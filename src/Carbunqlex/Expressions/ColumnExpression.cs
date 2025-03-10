﻿using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Expressions;

public class ColumnExpression : IValueExpression
{
    public List<string> Namespaces { get; set; }
    public string ColumnName { get; set; }

    public string NamespaceFullName => string.Join(".", Namespaces);

    public ColumnExpression(string columnName)
    {
        Namespaces = new List<string>();
        ColumnName = columnName;
    }

    public ColumnExpression(string tableName, string columnName)
    {
        if (!string.IsNullOrEmpty(tableName))
        {
            Namespaces = new List<string> { tableName };
        }
        else
        {
            Namespaces = new List<string>();
        }
        ColumnName = columnName;
    }

    public ColumnExpression(List<string> namespaces, string columnName)
    {
        Namespaces = namespaces;
        ColumnName = columnName;
    }

    public string DefaultName => ColumnName;

    public bool MightHaveQueries => false;

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        // ColumnExpression does not directly use CTEs, so return a single lexeme
        // e.g. "table.column"
        yield return new Token(TokenType.Identifier, ToSqlWithoutCte());
    }

    public string ToSqlWithoutCte()
    {
        if (string.IsNullOrWhiteSpace(ColumnName))
        {
            throw new InvalidOperationException("Column name cannot be null or empty.");
        }

        if (Namespaces.Count == 0)
        {
            return ColumnName;
        }

        var sb = new StringBuilder();
        sb.Append(NamespaceFullName);
        sb.Append(".");
        sb.Append(ColumnName);
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // ColumnExpression does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        yield return this;
    }
}
