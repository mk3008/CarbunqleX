using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. rows between 1 preceding and 1 following
/// </summary>
public static class WindowFrameParser
{
    private static string ParserName => nameof(WindowFrameParser);

    public static BetweenWindowFrame Parse(SqlTokenizer tokenizer)
    {
        var type = tokenizer.Read(ParserName, "rows", "range", "groups").CommandOrOperatorText;

        var next = tokenizer.Peek();
        if (next.CommandOrOperatorText == "between")
        {
            var value = BetweenWindowFrameBoundaryParser.Parse(tokenizer);
            return new BetweenWindowFrame(type, value);
        }
        else
        {
            var value = WindowFrameBoundaryParser.Parse(tokenizer);
            return new BetweenWindowFrame(type, value);
        }
    }
}
