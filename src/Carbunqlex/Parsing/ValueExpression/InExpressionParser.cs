using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class InExpressionParser
{
    private static string ParserName => nameof(InExpressionParser);

    public static InExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var isnNegated = tokenizer.Read(ParserName, TokenType.Command, token =>
        {
            return token.CommandOrOperatorText switch
            {
                "in" => false,
                "not in" => true,
                _ => throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(ParserName, "'in' or 'not in'", tokenizer, token)
            };
        });

        var args = ValueArgumentsParser.Parse(tokenizer, TokenType.OpenParen, TokenType.CloseParen);

        return new InExpression(isnNegated, left, args);
    }
}
