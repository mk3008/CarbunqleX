using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. between 1 preceding and 1 following
/// </summary>
internal static class BetweenWindowFrameBoundaryParser
{
    public static BetweenWindowFrameBoundary Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("between");
        var start = WindowFrameBoundaryExpressionParser.Parse(tokenizer);
        tokenizer.Read("and");
        var end = WindowFrameBoundaryExpressionParser.Parse(tokenizer);
        return new BetweenWindowFrameBoundary(start, end);
    }
}
