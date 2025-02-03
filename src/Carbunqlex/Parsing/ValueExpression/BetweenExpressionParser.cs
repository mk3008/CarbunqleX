using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class BetweenExpressionParser
{
    private static string ParserName => nameof(BetweenExpressionParser);

    public static BetweenExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var isNegated = tokenizer.Read(ParserName, TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "between" => false,
                "not between" => true,
                _ => throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(ParserName, "'between' or 'not between'", tokenizer, token)
            };
        });

        // In the case of "between", "and" should not be treated as an operator
        var start = ValueExpressionParser.Parse(tokenizer, ["and"]);
        tokenizer.Read(ParserName, "and");
        var end = ValueExpressionParser.Parse(tokenizer);
        return new BetweenExpression(isNegated, left, start, end);
    }
}
