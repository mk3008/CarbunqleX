using Carbunqlex.Parsing.ValueExpression;

namespace Carbunqlex.Parsing;

public class SqlTokenizer
{
    public SqlTokenizer(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql, nameof(sql));

        Memory = sql.AsMemory();
        PreviousToken = null;
        PeekPosition = 0;
    }

    /// <summary>
    /// The input string to tokenize.
    /// </summary>
    private ReadOnlyMemory<char> Memory { get; }

    /// <summary>
    /// The current position in the input string.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// The token that was peeked at.
    /// </summary>
    private Token? PeekToken;

    /// <summary>
    /// The position of the peeked token.
    /// </summary>
    private int PeekPosition;

    /// <summary>
    /// Indicates if the tokenizer has reached the end of the input string.
    /// </summary>
    public bool IsEnd => IsEndLogic();

    private bool IsEndLogic()
    {
        if (Position >= Memory.Length)
        {
            return true;
        }

        if (Memory.Span[Position] == ';')
        {
            // If the next token is a semicolon, move to the end of the text
            Position = Memory.Length;
            return true;
        }

        return false;
    }

    public Token? PreviousToken { get; private set; }

    public void CommitPeek()
    {
        if (PeekPosition == 0)
        {
            return;
        }
        Position = PeekPosition;
        PreviousToken = PeekToken;
        PeekPosition = 0;
        PeekToken = null;
    }

    public bool TryPeek(out Token token)
    {
        if (IsEnd)
        {
            token = Token.Empty;
            return false;
        }
        if (!PeekToken.HasValue)
        {
            PeekToken = Memory.ReadLexeme(PreviousToken, Position, out PeekPosition);
        }
        token = PeekToken.Value;
        return true;
    }

    public T Peek<T>(Func<Token, T> action)
    {
        if (TryPeek(out var token))
        {
            return action(token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(this);
    }

    public T Peek<T>(Func<Token, T> action, T defaultValue)
    {
        if (TryPeek(out var token))
        {
            return action(token);
        }
        return defaultValue;
    }

    public T Peek<T>(Func<SqlTokenizer, Token, T> action)
    {
        if (TryPeek(out var token))
        {
            return action(this, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(this);
    }

    public T Peek<T>(Func<SqlTokenizer, Token, T> action, T defaultValue)
    {
        if (TryPeek(out var token))
        {
            return action(this, token);
        }
        return defaultValue;
    }

    public Token Peek()
    {
        if (TryPeek(out var token))
        {
            return token;
        }
        throw SqlParsingExceptionBuilder.EndOfInput(this);
    }

    public bool TryRead(out Token token)
    {
        if (IsEnd)
        {
            token = Token.Empty;
            return false;
        }
        token = Read();
        return true;
    }

    private Token ReadCore()
    {
        // If we have a peeked token, return it and commit the peek
        if (PeekToken != null)
        {
            var cache = PeekToken.Value;
            CommitPeek();
            return cache;
        }

        var token = Memory.ReadLexeme(PreviousToken, Position, out var p);
        Position = p;
        return token;
    }

    public Token Read()
    {
        var token = ReadCore();
        PreviousToken = token;
        return token;
    }

    public Token Read(string expectedCommand)
    {
        if (TryRead(out var token))
        {
            if (token.CommandOrOperatorText == expectedCommand)
            {
                return token;
            }
            throw SqlParsingExceptionBuilder.UnexpectedToken(this, expectedCommand, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(this);
    }

    public Token Read(params string[] expectedCommands)
    {
        if (TryRead(out var token))
        {
            if (expectedCommands.Contains(token.CommandOrOperatorText))
            {
                return token;
            }
            throw SqlParsingExceptionBuilder.UnexpectedToken(this, expectedCommands, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(this);
    }

    public Token Read(IEnumerable<string> expectedCommands)
    {
        if (TryRead(out var token))
        {
            if (expectedCommands.Contains(token.CommandOrOperatorText))
            {
                return token;
            }
            throw SqlParsingExceptionBuilder.UnexpectedToken(this, expectedCommands, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(this);
    }

    public T Read<T>(TokenType expectedTokenType, Func<Token, T> action)
    {
        var token = Read(expectedTokenType);
        return action(token);
    }

    public Token Read(TokenType expectedTokenType)
    {
        if (TryRead(out var token))
        {
            if (token.Type == expectedTokenType)
            {
                return token;
            }

            throw SqlParsingExceptionBuilder.UnexpectedTokenType(this, expectedTokenType, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(this);
    }

    public Token Read(params TokenType[] expectedTokenTypes)
    {
        if (TryRead(out var token))
        {
            if (expectedTokenTypes.Contains(token.Type))
            {
                return token;
            }
            throw SqlParsingExceptionBuilder.UnexpectedTokenType(this, expectedTokenTypes, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(this);
    }
}
