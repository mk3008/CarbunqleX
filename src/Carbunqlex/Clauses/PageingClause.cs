using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class PagingClause : IPagingClause
{
    public IValueExpression Offset { get; }
    public IValueExpression Fetch { get; }

    public PagingClause(IValueExpression offset, IValueExpression fetch)
    {
        Offset = offset;
        Fetch = fetch;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append("offset ")
          .Append(Offset.ToSql())
          .Append(" rows fetch next ")
          .Append(Fetch.ToSql())
          .Append(" rows only");
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
            {
                new Lexeme(LexType.Keyword, "offset")
            };

        lexemes.AddRange(Offset.GetLexemes());
        lexemes.Add(new Lexeme(LexType.Keyword, "rows"));
        lexemes.Add(new Lexeme(LexType.Keyword, "fetch next"));
        lexemes.AddRange(Fetch.GetLexemes());
        lexemes.Add(new Lexeme(LexType.Keyword, "rows only"));

        return lexemes;
    }
}
