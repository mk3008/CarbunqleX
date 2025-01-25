using System.Text;

namespace Carbunqlex.Clauses;

public class FilterClause : IFunctionModifier
{
    public WhereClause WhereClause { get; set; }

    public OverClause? OverClause { get; set; }

    public bool MightHaveQueries => true;

    public FilterClause(WhereClause whereClause)
    {
        WhereClause = whereClause;
        OverClause = null;
    }

    public FilterClause(WhereClause whereClause, OverClause overClause)
    {
        WhereClause = whereClause;
        OverClause = overClause;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append($"filter({WhereClause.ToSqlWithoutCte()})");
        if (OverClause != null)
        {
            sb.Append(" ").Append(OverClause.ToSqlWithoutCte());
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "filter");
        yield return new Token(TokenType.OpenParen, "(");
        foreach (var token in WhereClause.GenerateTokensWithoutCte())
        {
            yield return token;
        }
        yield return new Token(TokenType.CloseParen, ")");

        if (OverClause != null)
        {
            foreach (var token in OverClause.GenerateTokensWithoutCte())
            {
                yield return token;
            }
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return WhereClause.GetQueries();
    }
}
