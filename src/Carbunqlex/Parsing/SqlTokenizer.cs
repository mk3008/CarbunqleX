using Carbunqlex.Parsing.ValueExpression;

namespace Carbunqlex.Parsing;

public class SqlTokenizer
{
    public SqlTokenizer(string sql)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql, nameof(sql));

        var memory = sql.AsMemory();
        var position = 0;
        Token? current = null;
        Tokens = new List<(int, Token)>(sql.Length / 5); // Pre-allocate list with an estimated size
        while (!memory.IsEndOrTerminated(position))
        {
            current = memory.ReadLexeme(current, position, out position);
            Tokens.Add((position, current.Value));
        }

        Index = 0;
    }

    public int Index { get; private set; }

    public int Position => Tokens[Index - 1].Position;

    private List<(int Position, Token Value)> Tokens { get; }

    /// <summary>
    /// Indicates if the tokenizer has reached the end of the input string.
    /// </summary>
    public bool IsEnd => Tokens.Count <= Index ? true : false;

    internal Token? PreviousToken => Index == 0 ? null : Tokens[Index].Value;

    public void CommitPeek()
    {
        Index++;
    }

    public bool TryPeek(out Token token)
    {
        if (IsEnd)
        {
            token = Token.Empty;
            return false;
        }
        token = Tokens[Index].Value;
        return true;
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

    public Token Read()
    {
        var t = Tokens[Index].Value;
        Index++;
        return t;
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
}
