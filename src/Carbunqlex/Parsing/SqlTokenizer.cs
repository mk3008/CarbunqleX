using Carbunqlex.Parsing.ValueExpressionParsing;

namespace Carbunqlex.Parsing;

public class SqlTokenizer
{
    public SqlTokenizer(string sql)
    {
        Memory = sql.AsMemory();
    }

    private ReadOnlyMemory<char> Memory { get; }

    public int Position { get; private set; }

    private Token? PeekToken;
    private int PeekPosition;

    public bool IsEnd => Position >= Memory.Length;

    public void CommitPeek()
    {
        if (PeekPosition == 0)
        {
            return;
        }
        Position = PeekPosition;
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
            PeekToken = Peek(out PeekPosition);
        }
        token = PeekToken.Value;
        return true;
    }

    public Token Peek(out int position) => Memory.ReadLexeme(Position, out position);

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
        // If we have a peeked token, return it and commit the peek
        if (PeekToken != null)
        {
            var cache = PeekToken.Value;
            CommitPeek();
            return cache;
        }

        var token = Memory.ReadLexeme(Position, out var p);
        Position = p;
        return token;
    }

    public Token Read(string sender, string expectedIdentifier)
    {
        if (TryRead(out var token))
        {
            if (token.Identifier == expectedIdentifier)
            {
                return token;
            }
            throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(sender, expectedIdentifier, this, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(sender, this);
    }

    public Token Read(string sender, TokenType expectedTokenType)
    {
        if (TryRead(out var token))
        {
            if (token.Type == expectedTokenType)
            {
                return token;
            }
            throw SqlParsingExceptionBuilder.UnexpectedTokenType(sender, expectedTokenType, this, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(sender, this);
    }

    public Token Read(string sender, params TokenType[] expectedTokenTypes)
    {
        if (TryRead(out var token))
        {
            if (expectedTokenTypes.Contains(token.Type))
            {
                return token;
            }
            throw SqlParsingExceptionBuilder.UnexpectedTokenType(sender, expectedTokenTypes, this, token);
        }
        throw SqlParsingExceptionBuilder.EndOfInput(sender, this);
    }
}
