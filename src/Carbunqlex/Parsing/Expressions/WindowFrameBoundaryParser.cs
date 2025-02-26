using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Expressions;

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
