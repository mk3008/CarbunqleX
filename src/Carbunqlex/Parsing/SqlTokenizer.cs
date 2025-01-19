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

    public void SetPosition(int position)
    {
        Position = position;
    }

    public void CommitPosition()
    {
        if (PeekPosition == 0)
        {
            return;
        }
        Position = PeekPosition;
        PeekPosition = 0;
        PeekToken = null;
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
        var token = Memory.ReadLexeme(Position, out var p);
        Position = p;
        return token;
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
}
