using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpressionParsing;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. 1 preceding
/// </summary>
internal static class WindowFrameBoundaryExpressionParser
{
    private static string ParserName => nameof(WindowFrameBoundaryExpressionParser);

    public static IWindowFrameBoundaryExpression Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();

        if (next.Identifier == "current row")
        {
            tokenizer.Read();
            return new WindowFrameBoundaryKeyword(next.Identifier);
        }

        if (next.Identifier == "unbounded")
        {
            tokenizer.Read();
            next = tokenizer.Peek();
            if (next.Identifier == "preceding" || next.Identifier == "following")
            {
                tokenizer.Read();
                return new WindowFrameBoundaryKeyword("unbounded " + next.Identifier);
            }
            throw SqlParsingExceptionBuilder.UnexpectedToken(ParserName, ["preceding", "following"], tokenizer, next);
        }

        var rows = ValueExpressionParser.Parse(tokenizer);

        next = tokenizer.Peek();
        if (next.Identifier == "preceding" || next.Identifier == "following")
        {
            tokenizer.Read();
            return new WindowFrameBoundaryExpression(rows, next.Identifier);
        }
        throw SqlParsingExceptionBuilder.UnexpectedToken(ParserName, ["preceding", "following"], tokenizer, next);
    }
}
