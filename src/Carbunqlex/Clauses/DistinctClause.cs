using Carbunqlex.Lexing;

namespace Carbunqlex.Clauses;

public class DistinctClause : IDistinctClause
{
    public bool IsDistinct { get; } = true;

    public string ToSqlWithoutCte()
    {
        return "distinct";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "distinct");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // DistinctClause does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }
}
