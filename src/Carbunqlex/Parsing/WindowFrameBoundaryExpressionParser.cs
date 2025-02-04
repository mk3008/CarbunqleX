using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpressionParsing;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. 1 preceding
/// </summary>
internal static class WindowFrameBoundaryExpressionParser
{
    public static IWindowFrameBoundaryExpression Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();

        if (next.CommandOrOperatorText == "current row")
        {
            tokenizer.Read();
            return new WindowFrameBoundaryKeyword(next.CommandOrOperatorText);
        }

        if (next.CommandOrOperatorText == "unbounded")
        {
            tokenizer.Read();
            next = tokenizer.Peek();
            if (next.CommandOrOperatorText == "preceding" || next.CommandOrOperatorText == "following")
            {
                tokenizer.Read();
                return new WindowFrameBoundaryKeyword("unbounded " + next.CommandOrOperatorText);
            }
            throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["preceding", "following"], next);
        }

        var rows = ValueExpressionParser.Parse(tokenizer);

        next = tokenizer.Peek();
        if (next.CommandOrOperatorText == "preceding" || next.CommandOrOperatorText == "following")
        {
            tokenizer.Read();
            return new WindowFrameBoundaryExpression(rows, next.CommandOrOperatorText);
        }
        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["preceding", "following"], next);
    }
}
