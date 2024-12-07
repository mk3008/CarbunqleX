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

    public IEnumerable<Lexeme> GetLexemes()
    {
        foreach (var ns in Namespaces)
        {
            yield return new Lexeme(LexType.Identifier, ns);
            yield return new Lexeme(LexType.Dot, ".");
        }
        yield return new Lexeme(LexType.Identifier, ColumnName);
    }

    public string ToSql()
    {
        if (string.IsNullOrWhiteSpace(ColumnName))
        {
            throw new InvalidOperationException("Column name cannot be null or empty.");
        }

        var sb = new StringBuilder();
        if (Namespaces.Count > 0)
        {
            sb.Append(string.Join(".", Namespaces));
            sb.Append(".");
        }
        sb.Append(ColumnName);
        return sb.ToString();
    }
}
