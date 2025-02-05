using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class LikeExpressionParser
{
    public static LikeExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var (isNegated, keyword) = tokenizer.Read(TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "like" => (false, "like"),
                "not like" => (true, "like"),
                "ilike" => (false, "ilike"),
                "not ilike" => (true, "ilike"),
                _ => throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["like", "not like"], token)
            };
        });

        var right = ValueExpressionParser.Parse(tokenizer);

        if (tokenizer.Peek(static t => t.CommandOrOperatorText == "escape" ? true : false, false))
        {
            tokenizer.CommitPeek();
            var escapeOption = tokenizer.Read(TokenType.Constant).Value;
            return new LikeExpression(isNegated, keyword, left, right, escapeOption);
        }

        return new LikeExpression(isNegated, keyword, left, right);
    }
}
