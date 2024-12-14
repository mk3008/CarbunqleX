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

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("offset ")
          .Append(Offset.ToSqlWithoutCte())
          .Append(" rows fetch next ")
          .Append(Fetch.ToSqlWithoutCte())
          .Append(" rows only");
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        // Initial capacity is set to 6 to accommodate the following lexemes:
        // 1 for "offset", 1 for "rows", 1 for "fetch next", 1 for "rows only",
        // and 2 lexemes for the Offset and Fetch expressions.
        // e.g. "offset 5 rows fetch next 10 rows only"
        var lexemes = new List<Lexeme>(6)
        {
            new Lexeme(LexType.Keyword, "offset")
        };

        lexemes.AddRange(Offset.GenerateLexemesWithoutCte());
        lexemes.Add(new Lexeme(LexType.Keyword, "rows"));
        lexemes.Add(new Lexeme(LexType.Keyword, "fetch next"));
        lexemes.AddRange(Fetch.GenerateLexemesWithoutCte());
        lexemes.Add(new Lexeme(LexType.Keyword, "rows only"));

        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        if (Offset.MightHaveQueries)
        {
            queries.AddRange(Offset.GetQueries());
        }
        if (Fetch.MightHaveQueries)
        {
            queries.AddRange(Fetch.GetQueries());
        }

        return queries;
    }
}
