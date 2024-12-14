using System.Text;

namespace Carbunqlex.DatasourceExpressions;

public class SubQuerySource : IDatasource
{
    public IQuery Query { get; set; }
    public string Alias { get; set; }
    public ColumnAliases ColumnAliases { get; set; }

    public SubQuerySource(IQuery query, string alias)
    {
        Query = query;
        Alias = alias;
        ColumnAliases = new ColumnAliases(Enumerable.Empty<string>());
    }

    public SubQuerySource(IQuery query, string alias, IEnumerable<string> columnAliases)
    {
        Query = query;
        Alias = alias;
        ColumnAliases = new ColumnAliases(columnAliases);
    }

    public string ToSqlWithoutCte()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            throw new ArgumentException("Alias is required for a function source.", nameof(Alias));
        }
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(Query.ToSqlWithoutCte());
        sb.Append(") as ");
        sb.Append(Alias);
        sb.Append(ColumnAliases.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.OpenParen, "(")
        };
        lexemes.AddRange(Query.GenerateLexemesWithoutCte());
        lexemes.Add(new Lexeme(LexType.CloseParen, ")"));
        lexemes.Add(new Lexeme(LexType.Keyword, "as"));
        lexemes.Add(new Lexeme(LexType.Identifier, Alias));
        lexemes.AddRange(ColumnAliases.GenerateLexemesWithoutCte());
        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery> { Query };
        queries.AddRange(Query.GetQueries());
        return queries;
    }
}
