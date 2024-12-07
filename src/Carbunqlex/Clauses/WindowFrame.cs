using System.Text;

namespace Carbunqlex.Clauses;

public enum FrameType : byte
{
    Rows,
    Range
}

public class WindowFrame : ISqlComponent
{
    public WindowFrameBoundary Start { get; set; }
    public WindowFrameBoundary End { get; set; }
    public FrameType FrameType { get; set; }

    public WindowFrame(WindowFrameBoundary start, WindowFrameBoundary end, FrameType frameType)
    {
        Start = start;
        End = end;
        FrameType = frameType;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.Keyword, FrameType == FrameType.Rows ? "rows between" : "range between"),
        };
        lexemes.AddRange(Start.GetLexemes());
        lexemes.Add(new Lexeme(LexType.Keyword, "and"));
        lexemes.AddRange(End.GetLexemes());
        return lexemes;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append(FrameType == FrameType.Rows ? "rows" : "range");
        sb.Append(" between ");
        sb.Append(Start.ToSql());
        sb.Append(" and ");
        sb.Append(End.ToSql());
        return sb.ToString();
    }
}
