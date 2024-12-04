namespace Carbunqlex;

public readonly struct Lexeme(LexType type, string value)
{
    public LexType Type { get; } = type;
    public string Value { get; } = value;
}
