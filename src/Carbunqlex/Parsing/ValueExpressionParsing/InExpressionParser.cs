using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class InExpressionParser
{
    private static string ParserName => nameof(InExpressionParser);

    public static InExpression Parse(SqlTokenizer tokenizer, IValueExpression left)
    {
        var token = tokenizer.Read(ParserName, TokenType.Command);
        var isnNegated = token.Identifier switch
        {
            "in" => false,
            "not in" => true,
            _ => throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(ParserName, "'in' or 'not in'", tokenizer, token)
        };

        var args = ValueExpressionParser.ParseArguments(tokenizer, TokenType.OpenParen, TokenType.CloseParen);

        return new InExpression(isnNegated, left, new ValueArguments(args));
    }
}
