namespace Carbunqlex.Clauses;

public class WithinGroupClause : IFunctionModifier
{
    public OrderByClause OrderByClause { get; set; }

    public WithinGroupClause(OrderByClause orderByClause)
    {
        OrderByClause = orderByClause;
    }
    public bool MightHaveQueries => OrderByClause.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        return $"within group({OrderByClause.ToSqlWithoutCte()})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return new[]
        {
            new Token(TokenType.StartClause, "within group", "within group"),
            new Token(TokenType.OpenParen, "("),
        }.Concat(OrderByClause.GenerateTokensWithoutCte())
        .Append(new Token(TokenType.CloseParen, ")"))
        .Append(new Token(TokenType.EndClause, string.Empty, "within group"));
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return OrderByClause.MightHaveQueries ? OrderByClause.GetQueries() : Enumerable.Empty<ISelectQuery>();
    }
}
