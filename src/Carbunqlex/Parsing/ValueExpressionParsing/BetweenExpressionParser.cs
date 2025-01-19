using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class BetweenExpressionParser
{
    private static string Name => nameof(BetweenExpressionParser);

    public static BetweenExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        if (!tokenizer.TryRead(out var token))
        {
            throw SqlParsingExceptionBuilder.EndOfInput(Name, tokenizer);
        }

        var isnNegated = token.Identifier switch
        {
            "between" => false,
            "not between" => true,
            _ => throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(Name, "'between' or 'not between'", tokenizer, token)
        };

        var start = ValueExpressionParser.Parse(tokenizer);
        if (!tokenizer.TryRead(out token) || token.Identifier != "and")
        {
            throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(Name, "and", tokenizer, token);
        }
        var end = ValueExpressionParser.Parse(tokenizer);
        return new BetweenExpression(isnNegated, left, start, end);
    }
}
