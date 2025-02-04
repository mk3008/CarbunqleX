using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class LikeExpressionParser
{
    public static LikeExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var isnNegated = tokenizer.Read(TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "like" => false,
                "not like" => true,
                _ => throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["like", "not like"], token)
            };
        });

        var right = ValueExpressionParser.Parse(tokenizer);

        if (tokenizer.Peek(static t => t.CommandOrOperatorText == "escape" ? true : false, false))
        {
            tokenizer.CommitPeek();
            var escapeOption = tokenizer.Read(TokenType.Constant).Value;
            return new LikeExpression(isnNegated, left, right, escapeOption);
        }

        return new LikeExpression(isnNegated, left, right);
    }
}
