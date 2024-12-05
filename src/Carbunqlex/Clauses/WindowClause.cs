namespace Carbunqlex.Clauses;

public class WindowClause : ISqlComponent
{
    public string Alias { get; set; }
    public WindowFunction WindowFunction { get; }

    public WindowClause(string alias, WindowFunction windowFunction)
    {
        Alias = alias;
        WindowFunction = windowFunction;
    }

    public string ToSql()
    {
        var functionSql = WindowFunction.ToSql();
        return string.IsNullOrEmpty(functionSql) ? string.Empty : $"window {Alias} as ({functionSql})";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.StartClause, "window", "window"),
            new Lexeme(LexType.Identifier, Alias),
            new Lexeme(LexType.Keyword, "as"),
            new Lexeme(LexType.OpenParen, "(", "window")
        };

        lexemes.AddRange(WindowFunction.GetLexemes());

        lexemes.Add(new Lexeme(LexType.CloseParen, ")", "window"));
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "window"));

        return lexemes;
    }
}
