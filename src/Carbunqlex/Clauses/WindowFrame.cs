using System.Text;

namespace Carbunqlex.Clauses;

public enum FrameType : byte
{
    Rows,
    Range
}

public class WindowFrame : IWindowFrame
{
    public WindowFrameBoundary Start { get; set; }
    public WindowFrameBoundary End { get; set; }
    public FrameType FrameType { get; set; }

    public bool MightHaveCommonTableClauses => Start.MightHaveQueries || End.MightHaveQueries;

    public WindowFrame(WindowFrameBoundary start, WindowFrameBoundary end, FrameType frameType)
    {
        Start = start;
        End = end;
        FrameType = frameType;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, FrameType == FrameType.Rows ? "rows between" : "range between"),
        };
        tokens.AddRange(Start.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Command, "and"));
        tokens.AddRange(End.GenerateTokensWithoutCte());
        return tokens;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(FrameType == FrameType.Rows ? "rows" : "range");
        sb.Append(" between ");
        sb.Append(Start.ToSqlWithoutCte());
        sb.Append(" and ");
        sb.Append(End.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();
        queries.AddRange(Start.GetQueries());
        queries.AddRange(End.GetQueries());
        return queries;
    }
}
