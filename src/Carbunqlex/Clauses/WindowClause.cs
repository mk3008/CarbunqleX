using System.Text;

namespace Carbunqlex.Clauses;

public class WindowClause : ISqlComponent
{
    public List<WindowExpression> WindowExpressions { get; }

    public WindowClause(params WindowExpression[] windowExpressions)
    {
        WindowExpressions = windowExpressions.ToList();
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

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        if (WindowExpressions.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        // Estimate the initial capacity for the lexemes list.
        // Each window expression can return multiple lexemes, so we add a buffer.
        // Additionally, we add space for commas and the "window" keyword.
        int initialCapacity = WindowExpressions.Count * 6 + 1;
        var lexemes = new List<Lexeme>(initialCapacity)
        {
            new Lexeme(LexType.StartClause, "window", "window")
        };

        foreach (var windowExpression in WindowExpressions)
        {
            lexemes.AddRange(windowExpression.GenerateLexemesWithoutCte());
            lexemes.Add(new Lexeme(LexType.Comma, ",", "window"));
        }

        if (lexemes.Count > 1)
        {
            // Remove the last comma
            lexemes.RemoveAt(lexemes.Count - 1);
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "window"));
        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        return WindowExpressions.SelectMany(we => we.GetQueries());
    }
}
