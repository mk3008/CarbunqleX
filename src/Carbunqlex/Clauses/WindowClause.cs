using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class WindowClause : ISqlComponent
{
    public List<WindowExpression> WindowExpressions { get; }

    public WindowClause(params WindowExpression[] windowExpressions)
    {
        WindowExpressions = windowExpressions.ToList();
    }

    public WindowClause(List<WindowExpression> windowExpressions)
    {
        WindowExpressions = windowExpressions;
    }

    public string ToSqlWithoutCte()
    {
        if (WindowExpressions.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder("window ");
        sb.Append(string.Join(", ", WindowExpressions.Select(we => we.ToSqlWithoutCte())));
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (WindowExpressions.Count == 0)
        {
            return Enumerable.Empty<Token>();
        }

        // Estimate the initial capacity for the tokens list.
        // Each window expression can return multiple tokens, so we add a buffer.
        // Additionally, we add space for commas and the "window" keyword.
        int initialCapacity = WindowExpressions.Count * 6 + 1;
        var tokens = new List<Token>(initialCapacity)
        {
            new Token(TokenType.StartClause, "window", "window")
        };

        foreach (var windowExpression in WindowExpressions)
        {
            tokens.AddRange(windowExpression.GenerateTokensWithoutCte());
            tokens.Add(new Token(TokenType.Comma, ",", "window"));
        }

        if (tokens.Count > 1)
        {
            // Remove the last comma
            tokens.RemoveAt(tokens.Count - 1);
        }

        tokens.Add(new Token(TokenType.EndClause, string.Empty, "window"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return WindowExpressions.SelectMany(we => we.GetQueries());
    }

    public void Add(WindowExpression windowExpression)
    {
        WindowExpressions.Add(windowExpression);
    }

    public void AddRange(IEnumerable<WindowExpression> windowExpressions)
    {
        WindowExpressions.AddRange(windowExpressions);
    }
}
