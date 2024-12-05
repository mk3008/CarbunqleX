namespace Carbunqlex.DatasourceExpressions;

public class TableSource : IDatasource
{
    public List<string> Namespaces { get; set; } = new();
    public string TableName { get; set; }
    public string Alias { get; set; }

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

    public string ToSql()
    {
        var namespacePart = Namespaces.Any() ? $"{string.Join(".", Namespaces)}." : string.Empty;
        if (string.IsNullOrEmpty(Alias) || Alias == TableName)
        {
            return $"{namespacePart}{TableName}";
        }
        return $"{namespacePart}{TableName} as {Alias}";
    }

    public IEnumerable<Lexeme> GetLexemes()
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
}
