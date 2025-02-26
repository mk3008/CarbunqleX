using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Clauses;

public class OffsetClause : ISqlComponent
{
    public IValueExpression Offset { get; }

    public OffsetClause(IValueExpression offset)
    {
        Offset = offset;
    }

    public string ToSqlWithoutCte()
    {
        return $"offset {Offset.ToSqlWithoutCte()}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "offset")
        };
        tokens.AddRange(Offset.GenerateTokensWithoutCte());
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Offset.MightHaveQueries ? Offset.GetQueries() : Enumerable.Empty<ISelectQuery>();
    }
}
