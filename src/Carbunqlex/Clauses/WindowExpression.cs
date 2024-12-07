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

    public string ToSql()
    {
        if (string.IsNullOrWhiteSpace(Alias))
        {
            throw new ArgumentException("Alias is required for a window expression.", nameof(Alias));
        }

        var sb = new StringBuilder();
        sb.Append(Alias);
        sb.Append(" as (");
        sb.Append(WindowFunction.ToSql());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
            {
                new Lexeme(LexType.Identifier, Alias),
                new Lexeme(LexType.Keyword, "as"),
                new Lexeme(LexType.OpenParen, "(", "window")
            };

        lexemes.AddRange(WindowFunction.GetLexemes());

        lexemes.Add(new Lexeme(LexType.CloseParen, ")", "window"));

        return lexemes;
    }
}
