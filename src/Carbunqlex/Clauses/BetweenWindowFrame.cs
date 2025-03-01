using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class BetweenWindowFrame : IWindowFrame
{
    public IWindowFrameBoundary Value { get; set; }

    public string FrameType { get; set; }

    public bool MightHaveCommonTableClauses => false;

    public BetweenWindowFrame(string frameType, IWindowFrameBoundary value)
    {
        FrameType = frameType;
        Value = value;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, FrameType),
        };
        tokens.AddRange(Value.GenerateTokensWithoutCte());
        return tokens;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(FrameType);
        sb.Append(' ');
        sb.Append(Value.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();
        queries.AddRange(Value.GetQueries());
        return queries;
    }
}
