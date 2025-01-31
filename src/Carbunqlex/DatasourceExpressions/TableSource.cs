using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class TableSource : IDatasource
{
    public List<string> Namespaces { get; set; }

    public string TableName { get; set; }

    public string DefaultName => TableName;

    public string TableFullName => GetTableFullName();

    /// <summary>
    /// List of column names that the table has.
    /// Set this only if you want to use advanced analysis.
    /// </summary>
    public List<string> ColumnNames { get; } = new();

    public TableSource(IEnumerable<string> namespaces, string tableName)
    {
        Namespaces = namespaces.ToList();
        TableName = tableName;
    }

    public TableSource(string tableName)
    {
        Namespaces = new List<string>();
        TableName = tableName;
    }

    private string GetTableFullName()
    {
        var sb = new StringBuilder();
        if (Namespaces.Any())
        {
            sb.Append(string.Join(".", Namespaces));
            sb.Append(".");
        }
        sb.Append(TableName);
        return sb.ToString();
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
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>();

        if (Namespaces.Any())
        {
            foreach (var ns in Namespaces)
            {
                tokens.Add(new Token(TokenType.Identifier, ns));
                tokens.Add(new Token(TokenType.Dot, "."));
            }
        }

        tokens.Add(new Token(TokenType.Identifier, TableName));

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // TableSource does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<string> GetSelectableColumns()
    {
        return ColumnNames;
    }

    public bool TryGetSubQuery([NotNullWhen(true)] out ISelectQuery? subQuery)
    {
        subQuery = null;
        return false;
    }

    public bool TryGetTableName([NotNullWhen(true)] out string? tableFullName)
    {
        tableFullName = GetTableFullName();
        return true;
    }

    public bool TryGetUnionQuerySource([NotNullWhen(true)] out UnionQuerySource? unionQuerySource)
    {
        unionQuerySource = null;
        return false;
    }
}
