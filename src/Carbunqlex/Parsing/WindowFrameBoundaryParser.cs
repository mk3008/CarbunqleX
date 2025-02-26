using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. 1 preceding
/// </summary>
internal static class WindowFrameBoundaryParser
{
    public static WindowFrameBoundary Parse(SqlTokenizer tokenizer)
    {
        return new WindowFrameBoundary(WindowFrameBoundaryExpressionParser.Parse(tokenizer));
    }
}
