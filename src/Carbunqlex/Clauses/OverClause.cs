using System.Text;

namespace Carbunqlex.Clauses;

public class OverClause : ISqlComponent
{
    public IWindowFunction WindowFunction { get; set; }

    public bool MightHaveCommonTableClauses => WindowFunction.MightHaveCommonTableClauses;

    public OverClause(IWindowFunction windowFunction)
    {
        WindowFunction = windowFunction;
    }

    public OverClause() : this(EmptyWindowFunction.Instance)
    {
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("over (");
        sb.Append(WindowFunction.ToSqlWithoutCte());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.StartClause, "over", "over"),
            new Token(TokenType.OpenParen, "(", "over")
        };

        tokens.AddRange(WindowFunction.GenerateTokensWithoutCte());

        tokens.Add(new Token(TokenType.CloseParen, ")", "over"));
        tokens.Add(new Token(TokenType.EndClause, string.Empty, "over"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return WindowFunction.GetQueries();
    }
}
