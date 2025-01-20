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

        tokenizer.Read(ParserName, TokenType.OpenParen);

        var args = new List<IValueExpression>();

        while (true)
        {
            args.Add(ValueExpressionParser.Parse(tokenizer));

            token = tokenizer.Read(ParserName, TokenType.CloseParen, TokenType.Comma);
            if (token.Type == TokenType.CloseParen)
            {
                break;
            }
            continue;
        }

        return new InExpression(isnNegated, left, new ValueArguments(args));
    }
}
