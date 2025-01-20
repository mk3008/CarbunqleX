using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class LikeExpressionParser
{
    private static string ParserName => nameof(LikeExpressionParser);

    public static LikeExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var token = tokenizer.Read(ParserName, TokenType.Command);
        var isnNegated = token.Identifier switch
        {
            "like" => false,
            "not like" => true,
            _ => throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(ParserName, "'like' or 'not like'", tokenizer, token)
        };

        var right = ValueExpressionParser.Parse(tokenizer);

        return new LikeExpression(isnNegated, left, right);
    }
}
