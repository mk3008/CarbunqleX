using System.Text;

namespace Carbunqlex.Clauses;

public class WindowExpression : ISqlComponent
{
    public string Alias { get; set; }
    public WindowFunction WindowFunction { get; }

    public WindowExpression(string alias, WindowFunction windowFunction)
    {
        Alias = alias;
        WindowFunction = windowFunction;
    }

    public string ToSqlWithoutCte()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            throw new ArgumentException("Alias is required for a window expression.", nameof(Alias));
        }

        var sb = new StringBuilder();
        sb.Append(Alias);
        sb.Append(" as (");
        sb.Append(WindowFunction.ToSqlWithoutCte());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
            {
                new Token(TokenType.Identifier, Alias),
                new Token(TokenType.Keyword, "as"),
                new Token(TokenType.OpenParen, "(", "window")
            };

        tokens.AddRange(WindowFunction.GenerateTokensWithoutCte());

        tokens.Add(new Token(TokenType.CloseParen, ")", "window"));

        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return WindowFunction.GetQueries();
    }
}
