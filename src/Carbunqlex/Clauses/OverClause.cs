namespace Carbunqlex.Clauses;

public class OverClause : ISqlComponent
{
    public WindowFunction? WindowFunction { get; set; }

    public OverClause(WindowFunction? windowFunction = null)
    {
        WindowFunction = windowFunction;
    }

    public string ToSql()
    {
        return WindowFunction != null ? $"over ({WindowFunction.ToSql()})" : "over ()";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.StartClause, "over", "over"),
            new Lexeme(LexType.OpenParen, "(", "over")
        };

        if (WindowFunction != null)
        {
            lexemes.AddRange(WindowFunction.GetLexemes());
        }

        lexemes.Add(new Lexeme(LexType.CloseParen, ")", "over"));
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "over"));
        return lexemes;
    }
}
