using System.Text;

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
        if (string.IsNullOrWhiteSpace(Alias))
        {
            throw new ArgumentException("Alias is required for a function source.", nameof(Alias));
        }
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(Query.ToSql());
        sb.Append(") as ");
        sb.Append(Alias);
        sb.Append(ColumnAliases.ToSql());
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.OpenParen, "(")
        };

        lexemes.AddRange(Query.GetLexemes());

        lexemes.Add(new Lexeme(LexType.CloseParen, ")"));
        lexemes.Add(new Lexeme(LexType.Keyword, "as"));
        lexemes.Add(new Lexeme(LexType.Identifier, Alias));

        lexemes.AddRange(ColumnAliases.GetLexemes());

        return lexemes;
    }
}
