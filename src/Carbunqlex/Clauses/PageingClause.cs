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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        // Initial capacity is set to 6 to accommodate the following tokens:
        // 1 for "offset", 1 for "rows", 1 for "fetch next", 1 for "rows only",
        // and 2 tokens for the Offset and Fetch expressions.
        // e.g. "offset 5 rows fetch next 10 rows only"
        var tokens = new List<Token>(6)
        {
            new Token(TokenType.Keyword, "offset")
        };

        tokens.AddRange(Offset.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Keyword, "rows"));
        tokens.Add(new Token(TokenType.Keyword, "fetch next"));
        tokens.AddRange(Fetch.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Keyword, "rows only"));

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

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
