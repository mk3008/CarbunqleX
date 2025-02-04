using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class InExpressionParser
{
    public static InExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var isnNegated = tokenizer.Read(TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "in" => false,
                "not in" => true,
                _ => throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["in", "not in"], token)
            };
        });

        var args = ValueArgumentsParser.Parse(tokenizer, TokenType.OpenParen, TokenType.CloseParen);

        return new InExpression(isnNegated, left, args);
    }
}
