namespace Carbunqlex.DatasourceExpressions;

public class ValuesQuerySource : IDatasource
{
    public ValuesQuery Query { get; set; }
    public string Alias { get; set; }
    public ColumnAliases ColumnAliases { get; set; }

    public ValuesQuerySource(ValuesQuery query, string alias)
    {
        Query = query;
        Alias = alias;
        ColumnAliases = new ColumnAliases(Enumerable.Empty<string>());
    }

    public ValuesQuerySource(ValuesQuery query, string alias, IEnumerable<string> columnAliases)
    {
        Query = query;
        Alias = alias;
        ColumnAliases = new ColumnAliases(columnAliases);
    }

    public string ToSql()
    {
        var columnAliasesPart = ColumnAliases.ToSql();
        return $"({Query.ToSql()}) AS {Alias}{columnAliasesPart}";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.OpenParen, "(")
        };

        lexemes.AddRange(Query.GetLexemes());

        lexemes.Add(new Lexeme(LexType.CloseParen, ")"));
        lexemes.Add(new Lexeme(LexType.Keyword, "AS"));
        lexemes.Add(new Lexeme(LexType.Identifier, Alias));

        lexemes.AddRange(ColumnAliases.GetLexemes());

        return lexemes;
    }
}