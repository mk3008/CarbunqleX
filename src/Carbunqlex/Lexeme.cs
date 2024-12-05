namespace Carbunqlex;

public readonly struct Lexeme
{
    public Lexeme(LexType type, string value, string clause)
    {
        Type = type;
        Value = value;
        Clause = clause;
    }

    public Lexeme(LexType lexType, string value) : this(lexType, value, string.Empty)
    {
    }

    public LexType Type { get; }
    public string Value { get; }
    public string Clause { get; }
}
