using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class FrameBoundaryExpression : IWindowFrameBoundaryExpression
{
    public static FrameBoundaryExpression Preceding(IValueExpression rows) => new FrameBoundaryExpression(rows, "preceding");
    public static FrameBoundaryExpression Following(IValueExpression rows) => new FrameBoundaryExpression(rows, "following");

    public string BoundaryKeyword { get; }
    public IValueExpression Value { get; }

    private FrameBoundaryExpression(IValueExpression value, string boundaryKeyword)
    {
        BoundaryKeyword = boundaryKeyword;
        Value = value;
    }

    public bool MightHaveQueries => Value.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Value.ToSqlWithoutCte())
          .Append(' ')
          .Append(BoundaryKeyword);
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>(Value.GenerateTokensWithoutCte().Count() + 1);
        tokens.AddRange(Value.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Keyword, BoundaryKeyword));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Value.GetQueries();
    }
}
