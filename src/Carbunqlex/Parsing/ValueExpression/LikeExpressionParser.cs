using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class LikeExpressionParser
{
    private static string ParserName => nameof(LikeExpressionParser);

    public static LikeExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var isnNegated = tokenizer.Read(ParserName, TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "like" => false,
                "not like" => true,
                _ => throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(ParserName, "'like' or 'not like'", tokenizer, token)
            };
        });

        var right = ValueExpressionParser.Parse(tokenizer);

        if (tokenizer.Peek(t => t.CommandOrOperatorText == "escape" ? true : false, false))
        {
            tokenizer.CommitPeek();
            var escapeOption = tokenizer.Read(ParserName, TokenType.Constant).Value;
            return new LikeExpression(isnNegated, left, right, escapeOption);
        }

        return new LikeExpression(isnNegated, left, right);
    }
}
