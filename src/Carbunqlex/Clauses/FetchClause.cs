using Carbunqlex.Lexing;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class FetchClause : ILimitClause
{
    public string FetchType { get; }

    public IValueExpression FetchCount { get; }

    public bool IsPercentage { get; }

    public string FetchSuffix { get; }

    public bool IsLimit => false;

    public FetchClause(string fetchType, IValueExpression fetchCount, bool isPercentage, string fetchSuffix)
    {
        FetchType = fetchType;
        FetchCount = fetchCount;
        IsPercentage = isPercentage;
        FetchSuffix = fetchSuffix;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder("fetch ");
        sb.Append(FetchType).Append(' ').Append(FetchCount.ToSqlWithoutCte());

        if (IsPercentage)
        {
            sb.Append(" percent");
        }
        if (!string.IsNullOrWhiteSpace(FetchSuffix))
        {
            sb.Append(' ').Append(FetchSuffix);
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "fetch"),
            new Token(TokenType.Command, FetchType)
        };
        tokens.AddRange(FetchCount.GenerateTokensWithoutCte());
        if (IsPercentage)
        {
            tokens.Add(new Token(TokenType.Command, "percent"));
        }
        if (!string.IsNullOrWhiteSpace(FetchSuffix))
        {
            tokens.Add(new Token(TokenType.Command, FetchSuffix));
        }
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return FetchCount.MightHaveQueries ? FetchCount.GetQueries() : Enumerable.Empty<ISelectQuery>();
    }
}
