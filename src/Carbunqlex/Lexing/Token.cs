namespace Carbunqlex.Lexing;

/// <summary>
/// Represents a token in a SQL statement.
/// </summary>
public readonly struct Token
{
    public static readonly Token Empty = new Token(TokenType.Unknown, string.Empty, string.Empty);
    public static readonly Token Comma = new Token(TokenType.Comma, ",");
    public static readonly Token AsKeyword = new Token(TokenType.Command, "as");

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> struct.
    /// </summary>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="value">The value of the token.</param>
    /// <param name="rawValue">The raw value of the token.</param>
    /// <param name="commandText">The command text of the token.</param>
    public Token(TokenType tokenType, string value, string rawValue, string commandText)
    {
        Type = tokenType;
        Value = value;
        _raw = rawValue;
        CommandOrOperatorText = tokenType == TokenType.Command || tokenType == TokenType.Operator
                ? value.ToLower()
                : string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> struct.
    /// </summary>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="value">The value of the token.</param>
    /// <param name="commandText">The command text of the token.</param>
    public Token(TokenType tokenType, string value, string commandText)
    {
        Type = tokenType;
        Value = value;
        _raw = null;
        CommandOrOperatorText = tokenType == TokenType.Command || tokenType == TokenType.Operator
                ? value.ToLower()
                : string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Token"/> struct.
    /// </summary>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="value">The value of the token.</param>
    public Token(TokenType tokenType, string value)
    {
        Type = tokenType;
        Value = value;
        _raw = null;
        CommandOrOperatorText = tokenType == TokenType.Command || tokenType == TokenType.Operator
                ? value.ToLower()
                : string.Empty;
    }

    /// <summary>
    /// The type of token.
    /// </summary>
    public TokenType Type { get; }
    /// <summary>
    /// The value of the token.
    /// Returns the value excluding comments and whitespace.
    /// </summary>
    public string Value { get; }
    /// <summary>
    /// The command or operator text of the token.
    /// Returns the lowercase text if it is a command or operator, otherwise an empty string.
    /// </summary>
    public string CommandOrOperatorText { get; }
    private readonly string? _raw = null;
    /// <summary>
    /// The raw value of the token.
    /// Returns the value including comments and whitespace.
    /// </summary>
    public string RawValue => _raw ?? Value;
}
