using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class WindowFrameBoundaryExpression : IWindowFrameBoundaryExpression
{
    public string BoundaryKeyword { get; }
    public IValueExpression Value { get; }

    public WindowFrameBoundaryExpression(IValueExpression value, string boundaryKeyword)
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
        tokens.Add(new Token(TokenType.Command, BoundaryKeyword));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Value.GetQueries();
    }
}
