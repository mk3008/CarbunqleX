namespace Carbunqlex;

public readonly struct Token
{
    public static readonly Token Empty = new Token(TokenType.Unknown, string.Empty, string.Empty);
    public static readonly Token Comma = new Token(TokenType.Comma, ",");
    public static readonly Token AsKeyword = new Token(TokenType.Command, "as");

    public Token(TokenType type, string value, string rawValue, string identifier)
    {
        Type = type;
        Value = value;
        _raw = rawValue;
        Identifier = identifier.ToLower();
    }

    public Token(TokenType type, string value, string identifier)
    {
        Type = type;
        Value = value;
        Identifier = identifier.ToLower();
    }

    public Token(TokenType tokenType, string value) : this(tokenType, value, value)
    {
    }

    public TokenType Type { get; }
    public string Value { get; }
    public string Identifier { get; }
    private readonly string? _raw = null;
    public string RawValue => _raw ?? Value;
}

//public static class TokenBuilder
//{
//    public static IList<Token> BuildClause(string name, Func<IList<Token>> buildAction)
//    {
//        var inner = buildAction();
//        var tokens = new List<Token>(inner.Count + 2)
//        {
//            new Token(TokenType.StartClause, name, name)
//        };
//        tokens.AddRange(inner);
//        tokens.Add(new Token(TokenType.EndClause, string.Empty, name));

//        return tokens;
//    }

//    public static IList<Token> BuildClause(string name, Func<IEnumerable<Token>> buildAction)
//    {
//        var inner = buildAction().ToList();
//        return BuildClause(name, () => inner);
//    }
//}
