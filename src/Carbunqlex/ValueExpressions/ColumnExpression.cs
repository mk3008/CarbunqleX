using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class ColumnExpression : IValueExpression
{
    public List<string> Namespaces { get; set; }
    public string ColumnName { get; set; }

    public ColumnExpression(string columnName)
    {
        Namespaces = new List<string>();
        ColumnName = columnName;
    }

    public ColumnExpression(string tableName, string columnName)
    {
        Namespaces = new List<string> { tableName };
        ColumnName = columnName;
    }

    public ColumnExpression(List<string> namespaces, string columnName)
    {
        Namespaces = namespaces;
        ColumnName = columnName;
    }

    public string DefaultName => ColumnName;

    public bool MightHaveCommonTableClauses => false;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        // ColumnExpression does not directly use CTEs, so return a single lexeme
        // e.g. "table.column"
        yield return new Lexeme(LexType.Identifier, ToSqlWithoutCte());
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
        sb.Append(string.Join(".", Namespaces));
        sb.Append(".");
        sb.Append(ColumnName);
        return sb.ToString();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        // ColumnExpression does not directly use CTEs, so return an empty list
        return Enumerable.Empty<CommonTableClause>();
    }
}
