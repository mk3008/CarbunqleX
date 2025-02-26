using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. rows between 1 preceding and 1 following
/// </summary>
public static class WindowFrameParser
{
    public static BetweenWindowFrame Parse(SqlTokenizer tokenizer)
    {
        var type = tokenizer.Read("rows", "range", "groups").CommandOrOperatorText;

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
