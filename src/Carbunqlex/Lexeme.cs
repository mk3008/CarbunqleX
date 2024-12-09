namespace Carbunqlex;

public readonly struct Lexeme
{
    public static readonly Lexeme Comma = new Lexeme(LexType.Comma, ",");
    public static readonly Lexeme AsKeyword = new Lexeme(LexType.Keyword, "as");

    public Lexeme(LexType type, string value, string clause)
    {
        Type = type;
        Value = value;
        Identifier = clause.ToLower();
    }

    public Lexeme(LexType lexType, string value) : this(lexType, value, value)
    {
    }

    public LexType Type { get; }
    public string Value { get; }
    public string Identifier { get; }
}

public static class LexemeBuilder
{
    public static IList<Lexeme> BuildClause(string name, Func<IList<Lexeme>> buildAction)
    {
        var inner = buildAction();
        var lexemes = new List<Lexeme>(inner.Count + 2)
        {
            new Lexeme(LexType.StartClause, name, name)
        };
        lexemes.AddRange(inner);
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, name));

        return lexemes;
    }

    public static IList<Lexeme> BuildClause(string name, Func<IEnumerable<Lexeme>> buildAction)
    {
        var inner = buildAction().ToList();
        return BuildClause(name, () => inner);
    }
}
