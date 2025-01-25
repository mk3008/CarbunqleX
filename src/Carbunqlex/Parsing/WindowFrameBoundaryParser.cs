using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. 1 preceding
/// </summary>
internal static class WindowFrameBoundaryParser
{
    private static string ParserName => nameof(WindowFrameBoundaryParser);

    public static WindowFrameBoundary Parse(SqlTokenizer tokenizer)
    {
        return new WindowFrameBoundary(WindowFrameBoundaryExpressionParser.Parse(tokenizer));
    }
}
