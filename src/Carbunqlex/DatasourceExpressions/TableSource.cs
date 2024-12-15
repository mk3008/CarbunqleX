using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class TableSource : IDatasource
{
    public List<string> Namespaces { get; set; } = new();
    public string TableName { get; set; }
    public string Alias { get; set; }

    /// <summary>
    /// List of column names that the table has.
    /// Set this only if you want to use advanced analysis.
    /// </summary>
    public List<string> ColumnNames { get; set; } = new();

    public TableSource(IEnumerable<string> namespaces, string tableName, string alias)
    {
        Namespaces = namespaces.ToList();
        TableName = tableName;
        Alias = alias;
    }

    public TableSource(string tableName, string alias)
    {
        TableName = tableName;
        Alias = alias;
    }

    public TableSource(string tableName)
    {
        TableName = tableName;
        Alias = tableName;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        if (Namespaces.Any())
        {
            sb.Append(string.Join(".", Namespaces));
            sb.Append(".");
        }
        sb.Append(TableName);
        if (!string.IsNullOrEmpty(Alias) && Alias != TableName)
        {
            sb.Append(" as ");
            sb.Append(Alias);
        }
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>();

        if (Namespaces.Any())
        {
            foreach (var ns in Namespaces)
            {
                lexemes.Add(new Lexeme(LexType.Identifier, ns));
                lexemes.Add(new Lexeme(LexType.Dot, "."));
            }
        }

        lexemes.Add(new Lexeme(LexType.Identifier, TableName));

        if (!string.IsNullOrEmpty(Alias) && Alias != TableName)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "as"));
            lexemes.Add(new Lexeme(LexType.Identifier, Alias));
        }

        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        // TableSource does not directly use queries, so return an empty list
        return Enumerable.Empty<IQuery>();
    }

    public IEnumerable<ColumnExpression> GetSelectableColumns()
    {
        if (string.IsNullOrEmpty(Alias))
        {
            return Enumerable.Empty<ColumnExpression>();
        }
        return ColumnNames.Select(column => new ColumnExpression(Alias, column));
    }
}
