using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class LimitClause : ILimitClause
{
    public IValueExpression Limit { get; }

    public bool IsLimit => true;

    public LimitClause(IValueExpression limit)
    {
        Limit = limit;
    }

    public string ToSqlWithoutCte()
    {
        return $"limit {Limit.ToSqlWithoutCte()}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "limit")
        };
        tokens.AddRange(Limit.GenerateTokensWithoutCte());
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Limit.MightHaveQueries ? Limit.GetQueries() : Enumerable.Empty<ISelectQuery>();
    }
}
